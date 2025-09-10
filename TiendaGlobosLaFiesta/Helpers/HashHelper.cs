using System.Security.Cryptography;
using System.Text;

namespace TiendaGlobosLaFiesta
{
    public static class HashHelper
    {
        public static bool VerificarHash(string password, string hashAlmacenado)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return hashString == hashAlmacenado;
        }
    }
}
