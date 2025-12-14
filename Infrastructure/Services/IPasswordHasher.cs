using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
    }

    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int Iterations = 10000;
        private const int SaltSize = 32;
        private const int HashSize = 32;
        private const char Delimiter = ':';

        public string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[SaltSize];
                rng.GetBytes(salt);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    var hash = pbkdf2.GetBytes(HashSize);
                    var hashString = Convert.ToBase64String(hash);
                    var saltString = Convert.ToBase64String(salt);
                    return $"{Iterations}{Delimiter}{saltString}{Delimiter}{hashString}";
                }
            }
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                var parts = hash.Split(Delimiter);
                if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
                    return false;

                var salt = Convert.FromBase64String(parts[1]);
                var storedHash = Convert.FromBase64String(parts[2]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    var computedHash = pbkdf2.GetBytes(HashSize);
                    return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
