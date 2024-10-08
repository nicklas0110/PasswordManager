namespace PasswordManager.Entities
{
    public class User
    {
        public int Id { get; set; }       // Primary key
        public string Username { get; set; }   // Username for each user
        public string MasterPasswordHash { get; set; }  // Hash of the master password
        public virtual ICollection<PasswordEntry> PasswordEntries { get; set; }  // Navigation property
    }
}