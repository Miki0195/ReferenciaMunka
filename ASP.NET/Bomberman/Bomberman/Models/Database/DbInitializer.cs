using System.Collections.Generic;
using Bomberman.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Bomberman.Models.Database
{
    public class DbInitializer
    {
        public static void Initialize(BombermanDbContext context, IUserService service)
        {
            //Used instead of Migrate, bc of different Db engines
            context.Database.EnsureCreated();

            //If database engine is changed, and a new migration is added, use this
            //context.Database.Migrate();

            if (context.Users.Count() > 0)
                return;

            context.Users.Add(new User
            {
                Id = 1,
                Username = "test",
                Password = service.EncryptPassword("test"),
                Email = "test@testdomain.com"
            });
            context.Users.Add(new User
            {
                Id = 2,
                Username = "test2",
                Password = service.EncryptPassword("test"),
                Email = "test2@testdomain.com"
            });

            context.SaveChanges();
        }
    }
}
