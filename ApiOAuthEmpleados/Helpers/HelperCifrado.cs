using System.Security.Cryptography;
using System.Text;

namespace ApiOAuthEmpleados.Helpers
{
    public class HelperCifrado
    {
        private readonly byte[] Key;
        private readonly byte[] IV;

        // Pasamos un "secret" por el constructor (idealmente desde tu appsettings)
        public HelperCifrado(string secret)
        {
            // AES requiere una clave de tamaño exacto (ej. 32 bytes para AES-256).
            // Hashear el string nos asegura tener siempre 32 bytes válidos.
            using (SHA256 sha256 = SHA256.Create())
            {
                this.Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(secret));
            }
            
            // AES también requiere un Vector de Inicialización (IV) de 16 bytes.
            // Para simplificar el helper, tomamos los primeros 16 bytes del hash de la clave.
            this.IV = new byte[16];
            Array.Copy(this.Key, this.IV, 16);
        }

        public string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = this.Key;
                aesAlg.IV = this.IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        // Convertimos a Base64 para poder transportarlo fácilmente como string en el Token
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        public string DecryptString(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = this.Key;
                aesAlg.IV = this.IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(buffer))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}