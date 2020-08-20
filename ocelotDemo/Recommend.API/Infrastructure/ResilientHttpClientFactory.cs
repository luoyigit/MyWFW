using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Resilience;

namespace User.Identity.Infrastructure
{
    public class ResilientHttpClientFactory:IResilientHttpClientFactory
    {
        private readonly ILogger<ResilientHttpClient> _logger;
        //设置重试的次数
        private readonly int _retryCount;
        //失败几次后断路器打开
        private readonly int _exceptionsAllowedBeforeBreaking;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResilientHttpClientFactory(ILogger<ResilientHttpClient> logger, IHttpContextAccessor httpContextAccessor, int exceptionsAllowedBeforeBreaking = 5, int retryCount = 6)
        {
            _logger = logger;
            _exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
            _retryCount = retryCount;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public ResilientHttpClient CreateResilientHttpClient() => new ResilientHttpClient((origin) => CreatePolicies(), _logger, _httpContextAccessor);

        private AsyncPolicy[] CreatePolicies()
            => new AsyncPolicy[]
            {
                Policy
                    .Handle<HttpRequestException>()
                    .WaitAndRetryAsync(
                        _retryCount,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),//每次重试时间是2次方
                        (exception, timeSpan, retryCount, context) => {
                            // Add logic to be executed before each retry, such as logging
                            var msg = $"第 {retryCount} 次进行错误重试 " +
                                      $"of {context.PolicyKey} " +
                                      $"at {context.OperationKey}, " +
                                      $"due to: {exception}.";
                            _logger.LogWarning(msg);
                            _logger.LogDebug(msg);
                        }),
                Policy.Handle<HttpRequestException>()
                    .CircuitBreakerAsync(
                        _exceptionsAllowedBeforeBreaking, //失败几次后断路器打开
                        TimeSpan.FromMinutes(1),          //断路器打开时长
                        //断路器打开时执行
                        (exception,duration) =>
                        {
                            //断路器打开进行日志记录
                            _logger.LogWarning("****************断路器打开***************");
                        },
                        //断路器关闭
                        ()=>{

                            _logger.LogWarning("****************断路器关闭***************");
                        })
            };
    }
}