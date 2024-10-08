using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace PasswordManager.Entities
{
    public class PasswordDbContext : DbContext
    {
        public DbSet<PasswordEntry> PasswordEntries { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Get the path to the project directory dynamically
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

            // Use a path relative to the project directory
            string databasePath = Path.Combine(projectDirectory, "passwords.db");

            //Console.WriteLine($"Using database at: {databasePath}"); // Uncomment for debugging

            optionsBuilder.UseSqlite($"Data Source={databasePath}");
        }
    }
}