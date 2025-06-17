using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using UMT.IServices;

namespace UMT.Services
{
    public class UserSecretService : IUserSecretService
    {
        private readonly string _appDataFolder;

        public UserSecretService()
        {
            _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Constants.Common.AppName);
            Directory.CreateDirectory(_appDataFolder);
        }

        public void WriteSecret(string fileName, string data)
        {
            string filePath = Path.Combine(_appDataFolder, fileName);

            // Encrypt data using DPAPI
            byte[] encryptedData = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(data),
                null,
                DataProtectionScope.CurrentUser);

            File.WriteAllBytes(filePath, encryptedData);
        }

        public string ReadSecret(string fileName)
        {
            string filePath = Path.Combine(_appDataFolder, fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("The secret file does not exist.", filePath);

            // Read and decrypt data using DPAPI
            byte[] encryptedData = File.ReadAllBytes(filePath);
            byte[] decryptedData = ProtectedData.Unprotect(
                encryptedData,
                null,
                DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decryptedData);
            //return Encoding.UTF8.GetString(encryptedData);
        }

        public void ClearSecret(string fileName)
        {
            string filePath = Path.Combine(_appDataFolder, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
