using System.Security.Cryptography;
using System.Text;

public static class HashHelper
{
    public static string GenerarHash(string input)
    {
        using SHA256 sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        StringBuilder builder = new StringBuilder();
        foreach (var b in bytes)
            builder.Append(b.ToString("x2")); // minúsculas
        return builder.ToString();
    }

    public static bool VerificarHash(string input, string hashAlmacenado)
    {
        string nuevoHash = GenerarHash(input).Trim();
        return nuevoHash == hashAlmacenado.Trim();
    }
}