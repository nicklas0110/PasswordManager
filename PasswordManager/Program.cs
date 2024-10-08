using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Entities;
using PasswordManager.Services;

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

            // Check if the salt file exists
            if (!File.Exists(saltPath))
            {
                Console.WriteLine("No salt file found. Create a new master password:");
                string masterPassword = Console.ReadLine();

                // Generate a new salt and derive a key
                byte[] salt = GenerateSalt();
                key = authService.DeriveKey(masterPassword, salt);

                // Store the salt securely
                File.WriteAllBytes(saltPath, salt);
                Console.WriteLine("Salt file created and master password set.");
            }
            else
            {
                // Read the existing salt from the file
                Console.WriteLine("Enter your master password to unlock:");
                string masterPassword = Console.ReadLine();

                // Retrieve the stored salt
                byte[] salt = File.ReadAllBytes(saltPath);
                key = authService.DeriveKey(masterPassword, salt);
                Console.WriteLine("Database unlocked.");
            }

            // Print the key for debugging (remove this in production)
            Console.WriteLine($"Key: {Convert.ToBase64String(key)}");

            // Main loop for password management
            bool exit = false;
            while (!exit)
            {
                // Display options to the user
                Console.WriteLine("Choose an option: [1] Add Entry, [2] Read Entries, [3] Exit");
                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        AddEntry(key, encryptionService);
                        break;
                    case "2":
                        ReadEntries(key, encryptionService);
                        break;
                    case "3":
                        exit = true;
                        Console.WriteLine("Exiting program. Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please choose again.");
                        break;
                }
            }
        }

        private static void AddEntry(byte[] key, EncryptionService encryptionService)
        {
            using (var context = new PasswordDbContext())
            {
                Console.WriteLine("Enter the service name (e.g., Gmail):");
                string serviceName = Console.ReadLine();
                Console.WriteLine("Enter the username or email:");
                string username = Console.ReadLine();
                Console.WriteLine("Enter the password:");
                string password = Console.ReadLine();

                // Generate a new IV for encryption
                byte[] iv = encryptionService.GenerateInitializationVector();
                Console.WriteLine($"Generated IV (Encryption): {Convert.ToBase64String(iv)}"); // Debug output

                byte[] encryptedPassword = encryptionService.Encrypt(password, key, iv);

                PasswordEntry newEntry = new PasswordEntry
                {
                    ServiceName = serviceName,
                    Username = username,
                    EncryptedPassword = Convert.ToBase64String(encryptedPassword),
                    IV = Convert.ToBase64String(iv) // Store the IV for decryption
                };

                context.PasswordEntries.Add(newEntry);
                context.SaveChanges(); // Save changes to the database
                Console.WriteLine("Password entry added successfully.");
            }
        }

        private static void ReadEntries(byte[] key, EncryptionService encryptionService)
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
                        // Retrieve the stored IV for decryption
                        byte[] entryIv = Convert.FromBase64String(entry.IV);
                        Console.WriteLine($"Retrieved IV (Decryption): {Convert.ToBase64String(entryIv)}"); // Debug output

                        try
                        {
                            string decryptedPassword = encryptionService.Decrypt(Convert.FromBase64String(entry.EncryptedPassword), key, entryIv);
                            Console.WriteLine($"Service: {entry.ServiceName}, Username: {entry.Username}, Password: {decryptedPassword}");
                        }
                        catch (CryptographicException ex)
                        {
                            Console.WriteLine($"Decryption failed for service {entry.ServiceName}: {ex.Message}");
                        }
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
