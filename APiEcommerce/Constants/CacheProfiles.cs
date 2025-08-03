using System;
using Microsoft.AspNetCore.Mvc;

namespace APiEcommerce.Constants;

public class CacheProfiles
{

    public const string Default10 = "Default10";
    public const string Default60 = "Default60";

    public static readonly CacheProfile Profile10 = new()
    {
        Duration = 10,
    };
        public static readonly CacheProfile Profile60 = new()
    {
        Duration = 60,
    };
}
