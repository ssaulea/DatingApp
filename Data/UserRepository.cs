using System;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
{
    public async Task<MemberDto?> GetMemberAsync(string userName, bool isCurrentUser = false)
    {
        var query = context.Users
        .Where(x => x.UserName == userName)
        .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
        .AsQueryable();

        if (isCurrentUser)
            query = query.IgnoreQueryFilters();

        return await query.SingleOrDefaultAsync();
    }

    public async Task<PagedList<MemberDto>> GetMembers(UserParams userParams)
    {
        var query =  context.Users.AsQueryable();
        
        query = query.Where(x => x.UserName != userParams.CurrentUserName);

        if (userParams.Gender != null)
            query = query.Where(x => x.Gender == userParams.Gender);

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(- userParams.MaxAge));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(- userParams.MinAge));
        query = query.Where(x => minDob <= x.DateOfBirth && x.DateOfBirth <= maxDob);
        query = userParams.OrderBy switch 
        {
            "created" => query.OrderByDescending(x => x.Created),
            _ => query.OrderByDescending(x => x.LastActive)
        };

        return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider), userParams.PageNumber, userParams.PageSize);
    }

    public async Task<AppUser?> GetUserByPhotoId(int photoId)
    {
        return await context.Users
            .Include(x => x.Photos)
            .IgnoreQueryFilters()
            .Where(x => x.Photos.Any(p => p.Id == photoId))
            .FirstOrDefaultAsync();
    }

    public async Task<AppUser?> GetUserByUserIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string userName)
    {
        return await context.Users.Include(x => x.Photos.Where(x => x.IsApproved))
            .SingleOrDefaultAsync(x => x.UserName == userName);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await context.Users.Include(x => x.Photos)
            .ToListAsync();
    }

    public void Update(AppUser user)
    {
        context.Entry(user).State = EntityState.Modified;
    }
}
