using Microsoft.EntityFrameworkCore;
using NexusArena.API.Models;
using System.Text.RegularExpressions;

namespace NexusArena.API
{
    public class HashExistingPasswords
    {
        public static async Task HashAllPasswords(NexusArenaDbContext context)
        {
            var users = await context.Users.ToListAsync();
            bool anyUpdated = false;

            foreach (var user in users)
            {
                if (!user.PasswordHash.StartsWith("$2a$") && 
                    !user.PasswordHash.StartsWith("$2b$") && 
                    !user.PasswordHash.StartsWith("$2x$") && 
                    !user.PasswordHash.StartsWith("$2y$"))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                    anyUpdated = true;
                }
            }

            if (anyUpdated)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Successfully hashed {users.Count(u => u.PasswordHash.StartsWith("$2"))} passwords in database.");
            }
            else
            {
                Console.WriteLine("✅ All passwords are already hashed.");
            }
        }
    }
}
