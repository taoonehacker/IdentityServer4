// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// DI extension methods for adding IdentityServer
    /// </summary>
    public static class IdentityServerServiceCollectionExtensions
    {
        /// <summary>
        /// Creates a builder.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddIdentityServerBuilder(this IServiceCollection services)
        {
            return new IdentityServerBuilder(services);
        }

        /// <summary>
        /// Adds IdentityServer.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services)
        {
            // 依赖注入的入口
            // 主要流程：
            // 1.注入EndPoints,
            // 2.客户端http请求 
            // 3.EndPointRouter寻找匹配接口
            // 4.从容器中得到接口对应的处理类IEndPointHandler
            // 5.IEndPointHandler处理完毕返回IEndPointResult
            // 6.IEndPointResult执行完毕返回报文

            var builder = services.AddIdentityServerBuilder();

            builder
                .AddRequiredPlatformServices()  //注入平台服务
                .AddCookieAuthentication()      //注入Cookie服务
                .AddCoreServices()              //注入核心服务
                .AddDefaultEndpoints()          //注入接口
                .AddPluggableServices()         //注入可插拔服务
                .AddValidators()                //注入校验类
                .AddResponseGenerators()        //注入响应生成类
                .AddDefaultSecretParsers()      //注入默认密钥解析器
                .AddDefaultSecretValidators();  //注入默认密钥校验

            // provide default in-memory implementation, not suitable for most production scenarios
            builder.AddInMemoryPersistedGrants();

            return builder;
        }

        /// <summary>
        /// Adds IdentityServer.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="setupAction">The setup action.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, Action<IdentityServerOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddIdentityServer();
        }

        /// <summary>
        /// Adds the IdentityServer.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IdentityServerOptions>(configuration);
            return services.AddIdentityServer();
        }

        /// <summary>
        /// Configures the OpenIdConnect handlers to persist the state parameter into the server-side IDistributedCache.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="schemes">The schemes to configure. If none provided, then all OpenIdConnect schemes will use the cache.</param>
        public static IServiceCollection AddOidcStateDataFormatterCache(this IServiceCollection services, params string[] schemes)
        {
            services.AddSingleton<IPostConfigureOptions<OpenIdConnectOptions>>(
                svcs => new ConfigureOpenIdConnectOptions(
                    schemes,
                    svcs.GetRequiredService<IHttpContextAccessor>())
            );

            return services;
        }
    }
}