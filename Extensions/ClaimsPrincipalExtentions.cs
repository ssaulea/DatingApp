using System;
using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipalExtentions
{
    public static string GetUserName(this ClaimsPrincipal source)
    {
        var username = source.FindFirstValue(ClaimTypes.Name);

        return username ?? throw new Exception("Cannot get username from token");
    }

    public static int GetUserId(this ClaimsPrincipal source)
    {
        var userId = int.Parse(source.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("Cannot get username from token"));

        return userId;
    }
}
