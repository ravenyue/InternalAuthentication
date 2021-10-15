using System;

namespace AspNetCore.Authentication.Internal
{
    [Flags]
    public enum ClaimSource
    {
        Header = 1,
        Query = 2,
        HeaderAndQuery = Header | Query,
    }
}
