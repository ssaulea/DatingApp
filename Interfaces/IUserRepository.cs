using System;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserByUserIdAsync(int id);
    Task<AppUser?> GetUserByUsernameAsync(string userName);

    Task<PagedList<MemberDto>> GetMembers(UserParams userParams);
    Task<MemberDto?> GetMember(string userName);

}
