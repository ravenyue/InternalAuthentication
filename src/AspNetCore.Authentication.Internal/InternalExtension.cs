using Microsoft.AspNetCore.Authentication;
using System;

namespace AspNetCore.Authentication.Internal
{
    public static class InternalExtension
    {
        public static AuthenticationBuilder AddInternal(
            this AuthenticationBuilder builder)
            => builder.AddInternal(InternalDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddInternal(
            this AuthenticationBuilder builder,
            Action<InternalOptions> configureOptions)
             => builder.AddInternal(InternalDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddInternal(
           this AuthenticationBuilder builder,
           string authenticationScheme,
           Action<InternalOptions> configureOptions)
            => builder.AddInternal(authenticationScheme, null, configureOptions);

        public static AuthenticationBuilder AddInternal(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            string? displayName,
            Action<InternalOptions> configureOptions)
            => builder.AddScheme<InternalOptions, InternalHandler>(authenticationScheme, displayName, configureOptions);
    }
}
