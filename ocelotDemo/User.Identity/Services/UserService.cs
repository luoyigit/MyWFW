using DnsClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Resilience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.Identity.Models;

namespace User.Identity.Services
{
    public class UserService : IUserService
    {
        //private readonly HttpClient _httpClient;

        private readonly IHttpClient _httpClient;
        private readonly IDnsQuery _dnsQuery;
        private readonly ILogger<UserService> _logger;
        //private readonly ServiceDiscoveryOptions _serviceOption;

        //服务请求地址
        private readonly string QueryAction = "/api/users/check-or-create";

        // public UserService(HttpClient httpClient,IDnsQuery dnsQuery, IOptions<ServiceDiscoveryOptions> serviceOption)
        // {
        //     _httpClient = httpClient;
        //     _dnsQuery = dnsQuery ?? throw new ArgumentNullException(nameof(dnsQuery));
        //     _serviceOption = serviceOption.Value ?? throw new ArgumentNullException(nameof(serviceOption));
        // }
        public UserService(IHttpClient httpClient, IDnsQuery dnsQuery, ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _dnsQuery = dnsQuery ?? throw new ArgumentNullException(nameof(dnsQuery));
            _logger = logger;
            //_serviceOption = serviceOption.Value ?? throw new ArgumentNullException(nameof(serviceOption));
        }
        // public async Task<BaseUserInfo> GetOrCreateAsync(string phone)
        // {
        //     //service.consul 这个是从哪来的？？
        //     var result = await _dnsQuery.ResolveServiceAsync("service.consul", "UserApi");
        //     var addressList = result.First().AddressList;
        //     var address = addressList.Any() ? addressList.First().ToString() : (result.First().HostName??"127.0.0.1");
        //     var port = result.First().Port;
        //     var appUrl = $"http://{address}:{port}{QueryAction}";
        //     
        //     var form = new Dictionary<string, string> {{"phone", phone}};
        //     var content=new FormUrlEncodedContent(form);
        //     var response = await _httpClient.PostAsync(appUrl, content);
        //     if (response.StatusCode == HttpStatusCode.OK)
        //     {
        //         var userInfo =  JsonConvert.DeserializeObject<BaseUserInfo>(await response.Content.ReadAsStringAsync());
        //         return userInfo;
        //     }
        //
        //     return null;
        // }

        public async Task<BaseUserInfo> GetOrCreateAsync(string phone)
        {
            try
            {
                var form = new Dictionary<string, string> { { "phone", phone } };
                var queryUrl = await GetServiceUrlFromConsulAsync();
                var response = await _httpClient.PostAsync(queryUrl, form);
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = JsonConvert.DeserializeObject<BaseUserInfo>(await response.Content.ReadAsStringAsync());
                    return userInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GetOrCreateAsync 在重试后调用失败", ex.Message + ex.StackTrace);
                throw;
            }
            return null;
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
                       var appUrl = $"http://{address}:{port}{QueryAction}";
                       return appUrl;
                   });
            }
            catch (Exception ex)
            {
                _logger.LogError($"从Consul发现UserApi地址,在重试3次后失败" + ex.Message + ex.StackTrace);
                return "";
            }
        }
    }
}
