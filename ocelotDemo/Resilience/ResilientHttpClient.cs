using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Resilience
{
    /// <summary>
    /// 对Polly进行封装,是代理类具有错误重试、断路器、仓壁隔离等特性
    /// </summary>
    public class ResilientHttpClient : IHttpClient
    {
        /// <summary>
        /// Http代理类
        /// </summary>
        private readonly HttpClient _client;
        /// <summary>
        /// Polly包装（ConcurrentDictionary线程并发安全），进行本地缓存
        /// </summary>
        private readonly ConcurrentDictionary<string, AsyncPolicyWrap> _policyWrappers;
        /// <summary>
        /// 根据URL Origin创建Policy
        /// </summary>
        private readonly Func<string, IEnumerable<IAsyncPolicy>> _policyCreator;
        private readonly ILogger<ResilientHttpClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResilientHttpClient(Func<string, IEnumerable<IAsyncPolicy>> policyCreator, ILogger<ResilientHttpClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            //创建一个Policy包装器
            _policyWrappers = new ConcurrentDictionary<string, AsyncPolicyWrap>();

            _policyCreator = policyCreator;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<HttpResponseMessage> PostAsync<T>(string uri, T item, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            return DoPostPutAsync(HttpMethod.Post, uri, CreateHttpContent(item), authorizationToken, requestId, authorizationMethod);
        }

        public Task<HttpResponseMessage> PostAsync(string uri, Dictionary<string, string> form, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            return DoPostPutAsync(HttpMethod.Post, uri, CreateHttpContent(form), authorizationToken, requestId, authorizationMethod);
        }

        public Task<string> GetStringAsync(string uri, string authorizationToken = null, string authorizationMethod = "Bearer")
        {
            var origin = GetOriginFromUri(uri);

            return HttpInvoker(origin, async (c) =>
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

                SetAuthorizationHeader(requestMessage);

                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }

                var response = await _client.SendAsync(requestMessage);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                return await response.Content.ReadAsStringAsync();
            });
        }

        public Task<HttpResponseMessage> DeleteAsync(string uri, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            var origin = GetOriginFromUri(uri);

            return HttpInvoker(origin, async (c) =>
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);

                SetAuthorizationHeader(requestMessage);

                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }

                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestid", requestId);
                }

                return await _client.SendAsync(requestMessage);
            });
        }

        public Task<HttpResponseMessage> PutAsync<T>(string uri, T item, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            return DoPostPutAsync(HttpMethod.Put, uri, CreateHttpContent(item), authorizationToken, requestId, authorizationMethod);
        }

        public Task<HttpResponseMessage> PutAsync<T>(string uri, Dictionary<string, string> form, string authorizationToken = null, string requestId = null,
            string authorizationMethod = "Bearer")
        {
            return DoPostPutAsync(HttpMethod.Put, uri, CreateHttpContent(form), authorizationToken, requestId, authorizationMethod);
        }

        private Task<HttpResponseMessage> DoPostPutAsync(HttpMethod method, string uri, HttpContent httpContent,
            string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            if (method != HttpMethod.Post && method != HttpMethod.Put)
                throw new ArgumentException("Value must be either post or put", nameof(method));

            var origin = GetOriginFromUri(uri);

            return HttpInvoker(origin, async (c) =>
            {
                //构建请求
                var requestMessage = new HttpRequestMessage(method, uri);
                //Header添加Authorization
                SetAuthorizationHeader(requestMessage);
                requestMessage.Content = httpContent;
                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }
                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestid", requestId);
                }
                var response = await _client.SendAsync(requestMessage);
                //如果请求返回时500，抛出HttpRequestException 
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                return response;
            });
        }

        private HttpContent CreateHttpContent<T>(T item)
        {
            return new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
        }

        private HttpContent CreateHttpContent(Dictionary<string, string> form)
        {
            return new FormUrlEncodedContent(form);
        }

        private void SetAuthorizationHeader(HttpRequestMessage requestMessage)
        {
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                requestMessage.Headers.Add("Authorization", new List<string>() { authorizationHeader });
            }
        }

        private async Task<T> HttpInvoker<T>(string origin, Func<Context, Task<T>> action)
        {
            var normalizedOrigin = NormalizeOrigin(origin);
            if (!_policyWrappers.TryGetValue(normalizedOrigin, out AsyncPolicyWrap policyWrap))
            {
                policyWrap = Policy.WrapAsync(_policyCreator(normalizedOrigin).ToArray());
                _policyWrappers.TryAdd(normalizedOrigin, policyWrap);
            }
            return await policyWrap.ExecuteAsync(action, new Context(normalizedOrigin));
        }
        private string NormalizeOrigin(string origin)
        {
            return origin?.Trim().ToLower();
        }

        private string GetOriginFromUri(string uri)
        {
            var url = new Uri(uri);
            var origin = $"{url.Scheme}://{url.Host}:{url.Port}";
            return origin;
        }
    }
}
