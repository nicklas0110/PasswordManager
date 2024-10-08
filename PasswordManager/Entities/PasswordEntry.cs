namespace PasswordManager.Entities
{
    public class PasswordEntry
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        public string IV { get; set; }
    }
}