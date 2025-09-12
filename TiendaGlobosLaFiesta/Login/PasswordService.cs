// Archivo: PasswordService.cs

using BCrypt.Net; // El using está bien

namespace TiendaGlobosLaFiesta.Services
{
    public static class PasswordService
    {
        /// <summary>
        /// Crea un hash de una contraseña usando BCrypt, que incluye una sal generada automáticamente.
        /// </summary>
        /// <param name="password">La contraseña en texto plano.</param>
        /// <returns>El hash de la contraseña listo para ser almacenado.</returns>
        public static string HashPassword(string password)
        {
            // 🔹 CORRECCIÓN: Se llama a través de BCrypt.Net.BCrypt
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifica si una contraseña en texto plano coincide con un hash almacenado.
        /// </summary>
        /// <param name="password">La contraseña en texto plano ingresada por el usuario.</param>
        /// <param name="storedHash">El hash guardado en la base de datos.</param>
        /// <returns>True si la contraseña es válida, de lo contrario False.</returns>
        public static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                // 🔹 CORRECCIÓN: Se llama a través de BCrypt.Net.BCrypt
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            // 🔹 CORRECCIÓN: La excepción correcta es SaltParseException
            catch (BCrypt.Net.SaltParseException)
            {
                // Maneja el caso en que el hash de la BD no tenga el formato correcto de BCrypt
                return false;
            }
        }
    }
}