using System;
using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<bool> SaveAllAsync();
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserByUserIdAsync(int id);
    Task<AppUser?> GetUserByUsernameAsync(string userName);

    Task<IEnumerable<MemberDto>> GetMembers();
    Task<MemberDto?> GetMember(string userName);

}
