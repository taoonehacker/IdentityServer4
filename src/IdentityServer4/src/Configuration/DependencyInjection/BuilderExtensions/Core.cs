// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Configuration.DependencyInjection;
using IdentityServer4.Endpoints;
using IdentityServer4.Events;
using IdentityServer4.Hosting;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using static IdentityServer4.Constants;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting.FederatedSignOut;
using IdentityServer4.Services.Default;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder extension methods for registering core services
    /// </summary>
    public static class IdentityServerBuilderExtensionsCore
    {
        /// <summary>
        /// Adds the required platform services.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddRequiredPlatformServices(this IIdentityServerBuilder builder)
        {
            //注入HttpContext访问器
            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();            
            builder.Services.AddOptions();
            //配置类
            builder.Services.AddSingleton(
                resolver => resolver.GetRequiredService<IOptions<IdentityServerOptions>>().Value);
            builder.Services.AddHttpClient();

            return builder;
        }

        /// <summary>
        /// Adds the default cookie handlers and corresponding configuration
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddCookieAuthentication(this IIdentityServerBuilder builder)
        {
            //注入名称为idsrv的cookie认证架构
            builder.Services.AddAuthentication(IdentityServerConstants.DefaultCookieAuthenticationScheme)
                .AddCookie(IdentityServerConstants.DefaultCookieAuthenticationScheme)
                .AddCookie(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            builder.Services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, ConfigureInternalCookieOptions>();
            builder.Services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureInternalCookieOptions>();
            builder.Services.AddTransientDecorator<IAuthenticationService, IdentityServerAuthenticationService>();
            builder.Services.AddTransientDecorator<IAuthenticationHandlerProvider, FederatedSignoutAuthenticationHandlerProvider>();

            return builder;
        }

        /// <summary>
        /// Adds the default endpoints.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddDefaultEndpoints(this IIdentityServerBuilder builder)
        {
            //注入所有默认接口，包括接口名称和地址。请求进来之后，路由类EndpointRouter通过路由来寻找匹配的处理器
            builder.Services.AddTransient<IEndpointRouter, EndpointRouter>();

            //认证回调接口
            builder.AddEndpoint<AuthorizeCallbackEndpoint>(EndpointNames.Authorize, ProtocolRoutePaths.AuthorizeCallback.EnsureLeadingSlash());
            //认证接口
            builder.AddEndpoint<AuthorizeEndpoint>(EndpointNames.Authorize, ProtocolRoutePaths.Authorize.EnsureLeadingSlash());
            //检查会话接口
            builder.AddEndpoint<CheckSessionEndpoint>(EndpointNames.CheckSession, ProtocolRoutePaths.CheckSession.EnsureLeadingSlash());
            //设备认证接口
            builder.AddEndpoint<DeviceAuthorizationEndpoint>(EndpointNames.DeviceAuthorization, ProtocolRoutePaths.DeviceAuthorization.EnsureLeadingSlash());
            //设备
            builder.AddEndpoint<DiscoveryKeyEndpoint>(EndpointNames.Discovery, ProtocolRoutePaths.DiscoveryWebKeys.EnsureLeadingSlash());
            //元数据接口
            builder.AddEndpoint<DiscoveryEndpoint>(EndpointNames.Discovery, ProtocolRoutePaths.DiscoveryConfiguration.EnsureLeadingSlash());
            //结束会话回调接口
            builder.AddEndpoint<EndSessionCallbackEndpoint>(EndpointNames.EndSession, ProtocolRoutePaths.EndSessionCallback.EnsureLeadingSlash());
            //结束会话接口
            builder.AddEndpoint<EndSessionEndpoint>(EndpointNames.EndSession, ProtocolRoutePaths.EndSession.EnsureLeadingSlash());
            //查询令牌信息接口
            builder.AddEndpoint<IntrospectionEndpoint>(EndpointNames.Introspection, ProtocolRoutePaths.Introspection.EnsureLeadingSlash());
            //撤销令牌接口
            builder.AddEndpoint<TokenRevocationEndpoint>(EndpointNames.Revocation, ProtocolRoutePaths.Revocation.EnsureLeadingSlash());
            //发放令牌接口
            builder.AddEndpoint<TokenEndpoint>(EndpointNames.Token, ProtocolRoutePaths.Token.EnsureLeadingSlash());
            //查询用户信息接口
            builder.AddEndpoint<UserInfoEndpoint>(EndpointNames.UserInfo, ProtocolRoutePaths.UserInfo.EnsureLeadingSlash());

            return builder;
        }

        /// <summary>
        /// Adds the endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The name.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddEndpoint<T>(this IIdentityServerBuilder builder, string name, PathString path)
            where T : class, IEndpointHandler
        {
            builder.Services.AddTransient<T>();
            builder.Services.AddSingleton(new IdentityServer4.Hosting.Endpoint(name, path, typeof(T)));

            return builder;
        }

        /// <summary>
        /// Adds the core services.
        /// 注入核心服务
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddCoreServices(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTransient<ISecretsListParser, SecretParser>();
            builder.Services.AddTransient<ISecretsListValidator, SecretValidator>();
            builder.Services.AddTransient<ExtensionGrantValidator>();
            builder.Services.AddTransient<BearerTokenUsageValidator>();
            builder.Services.AddTransient<JwtRequestValidator>();

            builder.Services.AddTransient<ReturnUrlParser>();
            builder.Services.AddTransient<IdentityServerTools>();

            builder.Services.AddTransient<IReturnUrlParser, OidcReturnUrlParser>();
            builder.Services.AddScoped<IUserSession, DefaultUserSession>();
            builder.Services.AddTransient(typeof(MessageCookie<>));

            builder.Services.AddCors();
            builder.Services.AddTransientDecorator<ICorsPolicyProvider, CorsPolicyProvider>();

            return builder;
        }

        /// <summary>
        /// Adds the pluggable services.
        /// 注入可插拔服务
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddPluggableServices(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddTransient<IPersistedGrantService, DefaultPersistedGrantService>();
            builder.Services.TryAddTransient<IKeyMaterialService, DefaultKeyMaterialService>();
            builder.Services.TryAddTransient<ITokenService, DefaultTokenService>();
            builder.Services.TryAddTransient<ITokenCreationService, DefaultTokenCreationService>();
            builder.Services.TryAddTransient<IClaimsService, DefaultClaimsService>();
            builder.Services.TryAddTransient<IRefreshTokenService, DefaultRefreshTokenService>();
            builder.Services.TryAddTransient<IDeviceFlowCodeService, DefaultDeviceFlowCodeService>();
            builder.Services.TryAddTransient<IConsentService, DefaultConsentService>();
            builder.Services.TryAddTransient<ICorsPolicyService, DefaultCorsPolicyService>();
            builder.Services.TryAddTransient<IProfileService, DefaultProfileService>();
            builder.Services.TryAddTransient<IConsentMessageStore, ConsentMessageStore>();
            builder.Services.TryAddTransient<IMessageStore<LogoutMessage>, ProtectedDataMessageStore<LogoutMessage>>();
            builder.Services.TryAddTransient<IMessageStore<LogoutNotificationContext>, ProtectedDataMessageStore<LogoutNotificationContext>>();
            builder.Services.TryAddTransient<IMessageStore<ErrorMessage>, ProtectedDataMessageStore<ErrorMessage>>();
            builder.Services.TryAddTransient<IIdentityServerInteractionService, DefaultIdentityServerInteractionService>();
            builder.Services.TryAddTransient<IDeviceFlowInteractionService, DefaultDeviceFlowInteractionService>();
            builder.Services.TryAddTransient<IAuthorizationCodeStore, DefaultAuthorizationCodeStore>();
            builder.Services.TryAddTransient<IRefreshTokenStore, DefaultRefreshTokenStore>();
            builder.Services.TryAddTransient<IReferenceTokenStore, DefaultReferenceTokenStore>();
            builder.Services.TryAddTransient<IUserConsentStore, DefaultUserConsentStore>();
            builder.Services.TryAddTransient<IHandleGenerationService, DefaultHandleGenerationService>();
            builder.Services.TryAddTransient<IPersistentGrantSerializer, PersistentGrantSerializer>();
            builder.Services.TryAddTransient<IEventService, DefaultEventService>();
            builder.Services.TryAddTransient<IEventSink, DefaultEventSink>();
            builder.Services.TryAddTransient<IUserCodeService, DefaultUserCodeService>();
            builder.Services.TryAddTransient<IUserCodeGenerator, NumericUserCodeGenerator>();
            builder.Services.TryAddTransient<ILogoutNotificationService, LogoutNotificationService>();
            builder.Services.TryAddTransient<IBackChannelLogoutService, DefaultBackChannelLogoutService>();
            builder.Services.TryAddTransient<IResourceValidator, DefaultResourceValidator>();
            builder.Services.TryAddTransient<IScopeParser, DefaultScopeParser>();

            builder.AddJwtRequestUriHttpClient();
            builder.AddBackChannelLogoutHttpClient();

            builder.Services.AddTransient<IClientSecretValidator, ClientSecretValidator>();
            builder.Services.AddTransient<IApiSecretValidator, ApiSecretValidator>();

            builder.Services.TryAddTransient<IDeviceFlowThrottlingService, DistributedDeviceFlowThrottlingService>();
            builder.Services.AddDistributedMemoryCache();

            return builder;
        }

        /// <summary>
        /// Adds the validators.
        /// 注入校验类
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddValidators(this IIdentityServerBuilder builder)
        {
            // core
            builder.Services.TryAddTransient<IEndSessionRequestValidator, EndSessionRequestValidator>();
            builder.Services.TryAddTransient<ITokenRevocationRequestValidator, TokenRevocationRequestValidator>();
            builder.Services.TryAddTransient<IAuthorizeRequestValidator, AuthorizeRequestValidator>();
            builder.Services.TryAddTransient<ITokenRequestValidator, TokenRequestValidator>();
            builder.Services.TryAddTransient<IRedirectUriValidator, StrictRedirectUriValidator>();
            builder.Services.TryAddTransient<ITokenValidator, TokenValidator>();
            builder.Services.TryAddTransient<IIntrospectionRequestValidator, IntrospectionRequestValidator>();
            builder.Services.TryAddTransient<IResourceOwnerPasswordValidator, NotSupportedResourceOwnerPasswordValidator>();
            builder.Services.TryAddTransient<ICustomTokenRequestValidator, DefaultCustomTokenRequestValidator>();
            builder.Services.TryAddTransient<IUserInfoRequestValidator, UserInfoRequestValidator>();
            builder.Services.TryAddTransient<IClientConfigurationValidator, DefaultClientConfigurationValidator>();
            builder.Services.TryAddTransient<IDeviceAuthorizationRequestValidator, DeviceAuthorizationRequestValidator>();
            builder.Services.TryAddTransient<IDeviceCodeValidator, DeviceCodeValidator>();

            // optional
            builder.Services.TryAddTransient<ICustomTokenValidator, DefaultCustomTokenValidator>();
            builder.Services.TryAddTransient<ICustomAuthorizeRequestValidator, DefaultCustomAuthorizeRequestValidator>();
            
            return builder;
        }

        /// <summary>
        /// Adds the response generators.
        /// 注入生成响应类
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddResponseGenerators(this IIdentityServerBuilder builder)
        {
            builder.Services.TryAddTransient<ITokenResponseGenerator, TokenResponseGenerator>();
            builder.Services.TryAddTransient<IUserInfoResponseGenerator, UserInfoResponseGenerator>();
            builder.Services.TryAddTransient<IIntrospectionResponseGenerator, IntrospectionResponseGenerator>();
            builder.Services.TryAddTransient<IAuthorizeInteractionResponseGenerator, AuthorizeInteractionResponseGenerator>();
            builder.Services.TryAddTransient<IAuthorizeResponseGenerator, AuthorizeResponseGenerator>();
            builder.Services.TryAddTransient<IDiscoveryResponseGenerator, DiscoveryResponseGenerator>();
            builder.Services.TryAddTransient<ITokenRevocationResponseGenerator, TokenRevocationResponseGenerator>();
            builder.Services.TryAddTransient<IDeviceAuthorizationResponseGenerator, DeviceAuthorizationResponseGenerator>();

            return builder;
        }

        /// <summary>
        /// Adds the default secret parsers.
        /// 注入默认密钥解析器
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddDefaultSecretParsers(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTransient<ISecretParser, BasicAuthenticationSecretParser>();
            builder.Services.AddTransient<ISecretParser, PostBodySecretParser>();

            return builder;
        }

        /// <summary>
        /// Adds the default secret validators.
        /// 注入默认密钥校验
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddDefaultSecretValidators(this IIdentityServerBuilder builder)
        {
            builder.Services.AddTransient<ISecretValidator, HashedSharedSecretValidator>();

            return builder;
        }

        internal static void AddTransientDecorator<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            services.AddDecorator<TService>();
            services.AddTransient<TService, TImplementation>();
        }

        internal static void AddDecorator<TService>(this IServiceCollection services)
        {
            var registration = services.LastOrDefault(x => x.ServiceType == typeof(TService));
            if (registration == null)
            {
                throw new InvalidOperationException("Service type: " + typeof(TService).Name + " not registered.");
            }
            if (services.Any(x => x.ServiceType == typeof(Decorator<TService>)))
            {
                throw new InvalidOperationException("Decorator already registered for type: " + typeof(TService).Name + ".");
            }

            services.Remove(registration);

            if (registration.ImplementationInstance != null)
            {
                var type = registration.ImplementationInstance.GetType();
                var innerType = typeof(Decorator<,>).MakeGenericType(typeof(TService), type);
                services.Add(new ServiceDescriptor(typeof(Decorator<TService>), innerType, ServiceLifetime.Transient));
                services.Add(new ServiceDescriptor(type, registration.ImplementationInstance));
            }
            else if (registration.ImplementationFactory != null)
            {
                services.Add(new ServiceDescriptor(typeof(Decorator<TService>), provider =>
                {
                    return new DisposableDecorator<TService>((TService)registration.ImplementationFactory(provider));
                }, registration.Lifetime));
            }
            else
            {
                var type = registration.ImplementationType;
                var innerType = typeof(Decorator<,>).MakeGenericType(typeof(TService), registration.ImplementationType);
                services.Add(new ServiceDescriptor(typeof(Decorator<TService>), innerType, ServiceLifetime.Transient));
                services.Add(new ServiceDescriptor(type, type, registration.Lifetime));
            }
        }
    }
}