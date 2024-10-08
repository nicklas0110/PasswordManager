using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Entities;

namespace PasswordManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string databasePath = "passwords.db";
            string saltPath = "salt.dat";
            AuthenticationService authService = new AuthenticationService();
            EncryptionService encryptionService = new EncryptionService();
            byte[] key = null;
            byte[] iv = null;

            // Ensure the database schema is created
            using (var context = new PasswordDbContext())
            {
                context.Database.EnsureCreated(); // Create the database if it does not exist
            }

            // Check if database exists
            if (!File.Exists(databasePath))
            {
                Console.WriteLine("No database found. Create a new master password:");
                string masterPassword = Console.ReadLine();

                // Generate a salt and derive a key
                byte[] salt = GenerateSalt();
                key = authService.DeriveKey(masterPassword, salt);

                // Store the salt securely
                File.WriteAllBytes(saltPath, salt);
                Console.WriteLine("Database created and master password set.");
            }
            else
            {
                Console.WriteLine("Enter your master password to unlock:");
                string masterPassword = Console.ReadLine();

                // Retrieve the stored salt
                byte[] salt = File.ReadAllBytes(saltPath);
                key = authService.DeriveKey(masterPassword, salt);
                Console.WriteLine("Database unlocked.");
            }

            // Generate IV for encryption/decryption
            iv = encryptionService.GenerateInitializationVector();

            // Continue to password management (add, read, etc.)
            Console.WriteLine("Choose an option: [1] Add Entry, [2] Read Entries");
            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    AddEntry(key, iv, encryptionService);
                    break;
                case "2":
                    ReadEntries(key, iv, encryptionService);
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }

        private static void AddEntry(byte[] key, byte[] iv, EncryptionService encryptionService)
{
    using (var context = new PasswordDbContext())
    {
        Console.WriteLine("Enter the service name (e.g., Gmail):");
        string serviceName = Console.ReadLine();
        Console.WriteLine("Enter the username or email:");
        string username = Console.ReadLine();
        Console.WriteLine("Enter the password:");
        string password = Console.ReadLine();

        // Generate a new IV for each encryption operation
        iv = encryptionService.GenerateInitializationVector();
        byte[] encryptedPassword = encryptionService.Encrypt(password, key, iv);

        // Store the IV as part of the PasswordEntry, convert IV to Base64
        PasswordEntry newEntry = new PasswordEntry
        {
            ServiceName = serviceName,
            Username = username,
            EncryptedPassword = Convert.ToBase64String(encryptedPassword),
            IV = Convert.ToBase64String(iv) // Add a new column "IV" to the PasswordEntry model
        };

        context.PasswordEntries.Add(newEntry);
        context.SaveChanges();
        Console.WriteLine("Password entry added successfully.");
    }
}

private static void ReadEntries(byte[] key, byte[] iv, EncryptionService encryptionService)
{
    using (var context = new PasswordDbContext())
    {
        var entries = context.PasswordEntries.ToList();
        if (entries.Count == 0)
        {
            Console.WriteLine("No entries found.");
        }
        else
        {
            Console.WriteLine("Stored entries:");
            foreach (var entry in entries)
            {
                // Retrieve the IV from the database and convert it from Base64
                byte[] entryIv = Convert.FromBase64String(entry.IV);
                string decryptedPassword = encryptionService.Decrypt(Convert.FromBase64String(entry.EncryptedPassword), key, entryIv);
                Console.WriteLine($"Service: {entry.ServiceName}, Username: {entry.Username}, Password: {decryptedPassword}");
            }
        }
    }
}


        // Generate a salt for key derivation
        private static byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);
                return salt;
            }
        }
    }
}
