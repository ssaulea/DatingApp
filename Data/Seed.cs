using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(DataContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

        var option = new JsonSerializerOptions(){PropertyNameCaseInsensitive = true};

        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, option);

        if (users == null) return;

        users.ForEach(user => 
        {
            using var hmac = new HMACSHA256();

            user.UserName = user.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
            user.PasswordSalt = hmac.Key;

        });

        await context.Users.AddRangeAsync(users);

        await context.SaveChangesAsync();

    }

}
