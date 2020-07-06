using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace User.Identity
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>()
            {
                //new ApiResource("gateway_api","gateway service"),
                //new ApiResource("user_api", "user service"),
                //new ApiResource("contact_api","contact service"),
                //new ApiResource("project_api","project service"),
                //new ApiResource("recommend_api","recommend service") 
                new ApiResource("one_api", "one api"),
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return  new List<Client>()
            {
                new Client()
                {
                    ClientId = "web",
                    ClientSecrets = new List<Secret>(){new Secret("secret".Sha256())},
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    AllowOfflineAccess = true,
                    RequireClientSecret = false,
                    AllowedGrantTypes = new List<string>(){"sms_auth_code"},// 继承IExtensionGrantValidator
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowedScopes = new List<string>()
                    {
                        //"gateway_api",
                        //"user_api",
                        //"contact_api",
                        //"project_api",
                        //"recommend_api",
                        "one_api",
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess
                    }
                }
            };
        }

        public static List<TestUser> GetTestUsers()
        {
            return  new List<TestUser>()
            {
                new TestUser
                {
                    SubjectId="pwdClient",
                    Password = "123",
                    Username ="123"
                },
                // MVC 登录测试用户
                new TestUser
                {
                    SubjectId="10000",
                    Password = "admin",
                    Username ="admin",
                    Claims = new Claim[]{
                        new Claim("permission","home.read"), 
                        new Claim("permission","home.write"),
                        new Claim("permission","home.delete"), 
                        new Claim("role","Admin"), 
                        new Claim("website", "http://www.yulu618.com")
                    }
                }
            };
        }
        
        public static List<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {               
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }
    }
}