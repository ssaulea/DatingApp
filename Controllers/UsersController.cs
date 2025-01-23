using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(DataContext dataContext) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            var users = await dataContext.Users.ToListAsync();
            return users;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {
            var user = await dataContext.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            return user;
        }

        [HttpPost("SaveUser")]
        public async Task<ActionResult<AppUser>> SaveUser(string userName)
        {
            var user = await dataContext.Users.AddAsync(new AppUser{ UserName = userName });
            await dataContext.SaveChangesAsync();
            return user.Entity;
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await dataContext.Users.FindAsync(id);
            if (user == null)
                return NotFound();
            dataContext.Users.Remove(user);
            await dataContext.SaveChangesAsync();
            return Ok();
        }
}
}
