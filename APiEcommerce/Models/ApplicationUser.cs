using System;
using Microsoft.AspNetCore.Identity;

namespace APiEcommerce.Models;

public class ApplicationUser : IdentityUser
{
    public string? Name { get; set; }

}
