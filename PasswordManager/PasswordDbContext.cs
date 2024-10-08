using Microsoft.EntityFrameworkCore;
using PasswordManager.Entities;

namespace PasswordManager;

public class PasswordDbContext : DbContext
{
    public DbSet<PasswordEntry> PasswordEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=passwords.db");
    }
}
