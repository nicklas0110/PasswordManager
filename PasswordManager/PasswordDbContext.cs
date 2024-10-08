using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using PasswordManager.Entities;

namespace PasswordManager
{
    public class PasswordDbContext : DbContext
    {
        public DbSet<PasswordEntry> PasswordEntries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use the database in the project directory please please pleeeeeease
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            string databasePath = Path.Combine(projectDirectory, "passwords.db");

            // Print the path to ensure it is correct
            Console.WriteLine($"Using database at: {databasePath}");

            optionsBuilder.UseSqlite($"Data Source={databasePath}");
        }
    }
}