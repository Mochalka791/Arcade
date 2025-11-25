namespace Arcade.Data.Security
{
    public sealed class PasswordHasher
    {
        public string Hash(string password)
        {
            return password; 
        }
        public bool Verify(string password, string storedValue)
        {
            return string.Equals(password, storedValue);
        }
    }
}
//hascher -> später einrichten -> speichert Hashes im Format A aber Verify prüft Hashes im Format B, deshalb funktioniert LOGIN nicht