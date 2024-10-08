namespace PasswordManager.Entities
{
    public class PasswordEntry
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        public string IV { get; set; }

        public int UserId { get; set; } // Foreign key to User table
        public User User { get; set; }  // Navigation property
    }
}