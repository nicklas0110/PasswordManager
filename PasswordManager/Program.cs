using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Entities;
using PasswordManager.Services;

namespace PasswordManager
{
    class Program
    {
        static User currentUser;  // Track the currently logged-in user

        static void Main(string[] args)
        {
            string saltPath = "salt.dat"; // Path to the salt file
            AuthenticationService authService = new AuthenticationService();
            EncryptionService encryptionService = new EncryptionService();
            byte[] key = null;

            // Ensure the database schema is created
            using (var context = new PasswordDbContext())
            {
                context.Database.EnsureCreated(); // Create the database if it does not exist
            }

            // Main login or registration flow
            bool authenticated = false;
            string masterPassword = null;
            while (!authenticated)
            {
                Console.WriteLine("Choose an option: [1] Log In, [2] Create New User, [3] Exit");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        authenticated = LogIn(out masterPassword);
                        break;
                    case "2":
                        masterPassword = CreateNewUser();
                        authenticated = masterPassword != null; // Ensure successful creation
                        break;
                    case "3":
                        Console.WriteLine("Exiting program. Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please choose again.");
                        break;
                }
            }

            // Check if the salt file exists. If not, create a new salt.
            // Derive encryption key after login or user creation
            if (!File.Exists(saltPath))
            {
                byte[] newSalt = GenerateSalt();
                File.WriteAllBytes(saltPath, newSalt);
                Console.WriteLine("Generated new salt and saved to file.");
            }

            byte[] salt = File.ReadAllBytes(saltPath);
            key = DeriveEncryptionKey(masterPassword, salt);

            // Main loop for password management
            bool exit = false;
            while (!exit)
            {
                // Display options to the user
                Console.WriteLine("");
                Console.WriteLine("Choose an option: [1] Add Entry, [2] Read Entries, [3] Delete All Entries, [4] Generate Password, [5] Log Out");
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
                        DeleteAllEntries();
                        break;
                    case "4":
                        // Generate a password and print it to the user
                        string generatedPassword = GeneratePassword();
                        Console.WriteLine($"Generated Password: {generatedPassword}");
                        break;
                    case "5":
                        exit = true;
                        Console.WriteLine("Logging out...");
                        authenticated = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please choose again.");
                        break;
                }
            }
        }
        // Create a new user and store it in the database
        private static string CreateNewUser()
        {
            using (var context = new PasswordDbContext())
            {
                Console.WriteLine("Enter a username:");
                string username = Console.ReadLine();

                if (context.Users.Any(u => u.Username == username))
                {
                    Console.WriteLine("Username already exists. Please choose a different one.");
                    return null;
                }

                Console.WriteLine("Enter a master password:");
                string masterPassword = Console.ReadLine();

                // Hash the master password and create a new user
                string hashedMasterPassword = new AuthenticationService().HashPassword(masterPassword);
                currentUser = new User
                {
                    Username = username,
                    MasterPasswordHash = hashedMasterPassword
                };
                
                // Store the user in the database
                context.Users.Add(currentUser);
                context.SaveChanges();
                Console.WriteLine("User created successfully.");
                return masterPassword; // Return the master password
            }
        }

        // Log in the user if the credentials are correct
        private static bool LogIn(out string masterPassword)
        {
            using (var context = new PasswordDbContext())
            {
                Console.WriteLine("Enter your username:");
                string username = Console.ReadLine();
                Console.WriteLine("Enter your master password:");
                masterPassword = Console.ReadLine();

                // Retrieve the user from the database
                currentUser = context.Users.SingleOrDefault(u => u.Username == username);
                if (currentUser == null)
                {
                    Console.WriteLine("Username not found. Please try again.");
                    return false;
                }

                // Verify the entered password
                if (new AuthenticationService().VerifyPassword(masterPassword, currentUser.MasterPasswordHash))
                {
                    Console.WriteLine("Login successful.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Incorrect password. Please try again.");
                    return false;
                }
            }
        }
        
        // Modify AddEntry to allow password generation or manual entry
        private static void AddEntry(byte[] key, EncryptionService encryptionService)
        {
            using (var context = new PasswordDbContext())
            {
                Console.WriteLine("Enter the service name (e.g., Gmail):");
                string serviceName = Console.ReadLine();
                Console.WriteLine("Enter the username or email:");
                string username = Console.ReadLine();

                // Ask the user if they want to generate a password or enter one manually
                Console.WriteLine("Would you like to [1] Generate a password or [2] Enter your own?");
                string passwordChoice = Console.ReadLine();

                string password;
                if (passwordChoice == "1")
                {
                    // Generate a password and display it
                    password = GeneratePassword();
                    Console.WriteLine($"Generated Password: {password}");
                }
                else
                {
                    // Manually enter the password
                    Console.WriteLine("Enter the password:");
                    password = Console.ReadLine();
                }
                
                // Generate a new IV for encryption
                byte[] iv = encryptionService.GenerateInitializationVector();
                byte[] encryptedPassword = encryptionService.Encrypt(password, key, iv);

                // Create a new PasswordEntry and save it to the database
                PasswordEntry newEntry = new PasswordEntry
                {
                    ServiceName = serviceName,
                    Username = username,
                    EncryptedPassword = Convert.ToBase64String(encryptedPassword),
                    IV = Convert.ToBase64String(iv),
                    UserId = currentUser.Id  // Associate the entry with the current user
                };

                context.PasswordEntries.Add(newEntry);
                context.SaveChanges();
                Console.WriteLine("Password entry added successfully.");
            }
        }

        // Read and decrypt stored password entries for the current user
        private static void ReadEntries(byte[] key, EncryptionService encryptionService)
        {
            using (var context = new PasswordDbContext())
            {
                // Filter entries by current user
                var entries = context.PasswordEntries.Where(e => e.UserId == currentUser.Id).ToList();
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

        // Delete all entries for the current user
        private static void DeleteAllEntries()
        {
            using (var context = new PasswordDbContext())
            {
                var entries = context.PasswordEntries.Where(e => e.UserId == currentUser.Id).ToList();
                if (entries.Count == 0)
                {
                    Console.WriteLine("No entries to delete.");
                }
                else
                {
                    context.PasswordEntries.RemoveRange(entries);
                    context.SaveChanges();
                    Console.WriteLine("All entries deleted successfully.");
                }
            }
        }

        // Generate a password with both lowercase and uppercase letters, symbols and digits
        private static string GeneratePassword()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*+;:,.<>?";
            Random random = new Random();
            StringBuilder result = new StringBuilder();

            // Generate 4 groups of 5 characters separated by dashes
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    result.Append(chars[random.Next(chars.Length)]);
                }
                if (i < 3) result.Append('-'); // Add dashes between groups
            }

            return result.ToString();
        }

        private static byte[] GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);
                return salt;
            }
        }

        private static byte[] DeriveEncryptionKey(string masterPassword, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(masterPassword, salt, 100_000))
            {
                return pbkdf2.GetBytes(32);
            }
        }
    }
}
