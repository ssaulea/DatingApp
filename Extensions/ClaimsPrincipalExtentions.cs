using System;
using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipalExtentions
{
    public static string GetUserName(this ClaimsPrincipal source)
    {
        var username = source.FindFirstValue(ClaimTypes.NameIdentifier);

        return username ?? throw new Exception("Cannot get username from token");
    }
}
