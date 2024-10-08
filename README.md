# Secure Password Manager

## Project Overview
This project is a console-based secure password manager implemented in C#. It allows users to securely store and retrieve credentials for various services. Each user has a unique master password for authentication and can add, read, and delete password entries.

## Features
1. **User Management**: 
   - Create new users with a master password.
   - Authenticate users with the stored master password.

2. **Password Storage**:
   - Passwords are stored in a local SQLite database (`passwords.db`) located in the project directory.
   - AES-256 encryption is used for securely storing passwords.

3. **Password Management**:
   - Users can add new entries, read existing entries, and delete all entries.
   - Supports password generation in the format: `VVz7Q-W1iwO-6q6&O-,*5$A`, with lowercase and uppercase letters, symbols and digits.

4. **Security**:
   - Passwords are encrypted using AES-256.
   - Master passwords are hashed using SHA-256 before storing in the database.
   - Initialization vectors (IVs) are generated for each encrypted entry to ensure secure encryption.

## Setup Instructions
1. **Prerequisites**:
   - [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
   - [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

2. **Clone the Repository**:
   ```bash
   git clone https://github.com/yourusername/PasswordManager.git
   cd PasswordManager
   ```

3. **Build the Project**:
   ```bash
   dotnet build
   ```

4. **Apply Migrations**:
   Ensure the database schema is up-to-date:
   ```bash
   dotnet ef database update
   ```

5. **Run the Application**:
   ```bash
   dotnet run --project PasswordManager
   ```

6. **Creating a New User**:
   - Choose the option to create a new user.
   - Enter a username and a master password.

7. **Adding a Password Entry**:
   - After logging in, choose the option to add a new entry.
   - Choose whether to generate a password or enter one manually.

8. **Reading and Deleting Entries**:
   - Read stored entries for the logged-in user or delete all entries.

## Security Model
1. **Encryption**:
   - Passwords are encrypted using AES-256 before being stored in the database.
   - An Initialization Vector (IV) is generated for each password to ensure unique encryption, even for identical passwords.

2. **Key Management**:
   - A master password is used to derive the encryption key using a Key Derivation Function (KDF).
   - The master password is never stored directly in the database—only its hashed value using SHA-256.

3. **Database**:
   - The database (`passwords.db`) is stored locally in the project directory.
   - The database is structured with two main tables:
     - **Users**: Stores username and hashed master password.
     - **PasswordEntries**: Stores service name, username, encrypted password, IV, and user association.

## Security Considerations
- The application should be run in a secure environment where unauthorized users do not have access to the `passwords.db` file or the `salt.dat` file.
- The master password should be chosen to be strong enough to resist brute-force attacks.
- Backup the database regularly and keep it in a secure location.
