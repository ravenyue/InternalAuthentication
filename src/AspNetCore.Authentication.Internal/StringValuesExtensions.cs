using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace AspNetCore.Authentication.Internal
{
    internal static class StringValuesExtensions
    {
        public static IEnumerable<Claim> ToClaims(
            this StringValues values,
            string claimType,
            string delimiter)
        {
            if (values.Count == 0)
            {
                return Enumerable.Empty<Claim>();
            }
            var claims = values
                .SelectMany(val =>
                    val.Split(delimiter, StringSplitOptions.RemoveEmptyEntries))
                .Select(x => new Claim(claimType, x));

            return claims;
        }
    }
}
