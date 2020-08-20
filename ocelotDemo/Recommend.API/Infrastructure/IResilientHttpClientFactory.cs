using Resilience;

namespace User.Identity.Infrastructure
{
    public interface IResilientHttpClientFactory
    {
        /// <summary>
        /// 创建 ResilientHttpClient 实例
        /// </summary>
        /// <returns></returns>
        ResilientHttpClient CreateResilientHttpClient();
    }
}