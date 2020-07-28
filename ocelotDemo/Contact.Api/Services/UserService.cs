using System;
using System.Linq;
using System.Threading.Tasks;
using Contact.Api.Dtos;
using DnsClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Resilience;

namespace Contact.Api.Services
{
    public class UserService:IUserService
    {
        private readonly IHttpClient _httpClient;
        private readonly IDnsQuery _dnsQuery;
        private readonly ILogger<UserService> _logger;
        //服务请求地址
        private readonly string queryAction = "/api/users/get-userInfo/";
        
        public UserService(IHttpClient httpClient,IDnsQuery dnsQuery,ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _dnsQuery = dnsQuery ?? throw new ArgumentNullException(nameof(dnsQuery));;
            _logger = logger;
        }
        public async Task<BaseUserInfo> GetBaseUserInfoAsync(int userId)
        {
            var userApiAddress = await GetServiceUrlFromConsulAsync();
            var result =await _httpClient.GetStringAsync(userApiAddress + userId);
            return JsonConvert.DeserializeObject<BaseUserInfo>(result);
        }
        
        /// <summary>
        /// 从Consul发现用户服务地址
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetServiceUrlFromConsulAsync()
        {
            try
            {
                var policyRetry = Policy.Handle<InvalidOperationException>()
                      .WaitAndRetryAsync(
                      3,
                      retryTimespan => TimeSpan.FromSeconds(Math.Pow(2, retryTimespan)),
                      (exception, timespan, retryCount, context) =>
                      {
                          var msg = $"第 {retryCount} 次进行错误重试 " +
                                    $"of {context.PolicyKey} " +
                                    $"at {context.OperationKey}, " +
                                    $"due to: {exception}.";
                          _logger.LogWarning(msg);
                          _logger.LogDebug(msg);

                      });
                var policyBreak = Policy.Handle<InvalidOperationException>()
                                    .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1),
                                    (exception, timespan) =>
                                    {

                                        _logger.LogTrace("断路器打开");
                                    },
                                    () =>
                                    {
                                        _logger.LogTrace("断路器关闭");
                                    });

                var policyWary = Policy.WrapAsync(policyRetry, policyBreak);


               return await policyWary.ExecuteAsync(async () =>
                  {
                      var result = await _dnsQuery.ResolveServiceAsync("service.consul", "UserApi");
                      var addressList = result.First().AddressList;
                      var address = addressList.Any() ? addressList.First().ToString() : result.First().HostName;
                      var port = result.First().Port;
                      var appUrl = $"http://{address}:{port}{queryAction}";
                      return appUrl;
                  });
            }
            catch (Exception ex)
            {
                _logger.LogError($"从Consul发现UserApi地址,在重试3次后失败" + ex.Message +ex.StackTrace);
                return "";
            }
        }
    }
}