using System;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace APiEcommerce.Constants;

public static class PolicyName
{
    public const string AllowSpecifiOrigin = "AllowSpecifiOrigin";
    public const string SecretKey = "ApiSettings:SecretKey";

    internal static void AllowSpecificOrigin(CorsPolicyBuilder builder)
    {
        throw new NotImplementedException();
    }
}
