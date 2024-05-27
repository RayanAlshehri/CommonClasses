using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;

namespace CommonClasses
{
    public static class clsUtility
    {
        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }

        public static string FormatDateToDMY(DateTime Date)
        {
            return Date.ToString("dd/MMM/yyyy");
        }

        public static string FormatDateToDMYHM(DateTime Date)
        {
            return Date.ToString("dd/MMM/yyyy h:mm tt");
        }

        public static void RoundNumberToNextMultipleOf10(ref int Number)
        {
           if (Number % 10 != 0)
                Number = Number + (10 - (Number % 10));                   
        }

        public static void RoundNumberToPreviuosMultipleOf10(ref int Number)
        {
            if (Number % 10 != 0)
                Number = Number - (Number % 10);
        }

        public static byte GetAge(DateTime DateOfBirth)
        {
            DateTime CurrentDate = DateTime.Now;

            if (CurrentDate < DateOfBirth || DateOfBirth.Year == CurrentDate.Year)
                return 0;

            byte Age = Convert.ToByte(CurrentDate.Year - DateOfBirth.Year);

            if (DateOfBirth.Month > CurrentDate.Month)
                Age--;
            else if (DateOfBirth.Month == CurrentDate.Month && DateOfBirth.Day > CurrentDate.Day)
                Age--;

            return Age;
        }

        public static bool CreateFolderIfDoesNotExist(string FolderPath)
        {
            if (!Directory.Exists(FolderPath))
            {
                try
                {
                    Directory.CreateDirectory(FolderPath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        public static string ReplaceFileNameWithGUID(string SourceFile)
        {
            FileInfo FI = new FileInfo(SourceFile);
            return GenerateGUID() + FI.Extension;
        }

        public static string CopyFileToDestinationFileWithGUIDname(string SourceFile, string DestinationFile)
        {
            string NewPath = DestinationFile + "\\" + ReplaceFileNameWithGUID(SourceFile);

            File.Copy(SourceFile, NewPath);

            return NewPath;
        }

        public static bool StroreCredentialsInFile(string Username, string Password)
        {
            try
            {
                string CurrentDirectory = System.IO.Directory.GetCurrentDirectory();
                string FilePath = CurrentDirectory + "\\data.txt";

                if (Username == "" && File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                    return true;

                }

                using (StreamWriter Writer = new StreamWriter(FilePath))
                {
                    Writer.WriteLine(Username + "#//#" + Password);
                    return true;
                }
            }
            catch
            {
                return false;
            }

        }

        public static bool GetStoredCredentialsFromFile(ref string Username, ref string Password)
        {
            try
            {
                string CurrentDirectory = Directory.GetCurrentDirectory();
                string FilePath = CurrentDirectory + "\\data.txt";

                if (File.Exists(FilePath))
                {
                    using (StreamReader Reader = new StreamReader(FilePath))
                    {
                        string Line;
                        while ((Line = Reader.ReadLine()) != null)
                        {
                            string[] Result = Line.Split(new string[] { "#//#" }, StringSplitOptions.None);

                            Username = Result[0];
                            Password = Result[1];
                        }
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }

        public static bool StoreUsernameInRegestry(string Username, string KeyName)
        {
            try
            {
                Registry.SetValue($@"HKEY_CURRENT_USER\SOFTWARE\{KeyName}", "Username", Username);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool GetStoredUsernameInRegestry(ref string Username, string KeyName)
        {
            try
            {
                Username = null;
                Username = Registry.GetValue($@"HKEY_CURRENT_USER\SOFTWARE\{KeyName}", "Username", Username) as string;

                if (Username != null)
                    return true;
                
                return false;
            }
            catch 
            {
                return false;
            }
        }

        public static bool DeleteStoredUsernameInRegestry(string KeyName)
        {
            try
            {
                DeleteValueFromCurrentUserRegestry($@"SOFTWARE\{KeyName}", "Username");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteValueFromCurrentUserRegestry(string KeyPath, string ValueToDeleteName)
        {
            try
            {
                using (RegistryKey BaseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                {
                    using (RegistryKey Key = BaseKey.OpenSubKey(KeyPath, true))
                    {
                        if (Key != null)
                        {
                            Key.DeleteValue(ValueToDeleteName);
                            return true;
                        }

                        return false;
                    }
                }
            }
            catch 
            { 
                return false; 
            }
        }

        public static bool LogExceptionToEventViewer(string SourceName, Exception EX)
        {
            try
            {
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, "Application");
                }


                EventLog.WriteEntry(SourceName, $"Error {EX.Message}", EventLogEntryType.Error);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string HashData<T>(T Data)
        {
            try
            {
                using (SHA256 SHA = SHA256.Create())
                {
                    byte[] HashBytes = SHA.ComputeHash(Encoding.UTF8.GetBytes(Data.ToString()));
                    return BitConverter.ToString(HashBytes).Replace("-", "").ToLower();
                }
            }
            catch 
            {
                throw;
            }
        }

        public static byte[] GenerateKey()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.GenerateKey();
                return aes.Key;
            }
        }

        public static (byte[], byte[]) Encrypt(byte[] data, byte[] key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.CBC; // Use Cipher Block Chaining (CBC) mode
                aes.GenerateIV(); // Generate a random initialization vector (IV)

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(data, 0, data.Length);
                        csEncrypt.FlushFinalBlock();
                    }
                    return (msEncrypt.ToArray(), aes.IV);
                }
            }
        }

        public static byte[] Decrypt(byte[] EncryptedData, byte[] Key, byte[] IV)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC; // Use Cipher Block Chaining (CBC) mode

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        csDecrypt.Write(EncryptedData, 0, EncryptedData.Length);
                    }
                    return msDecrypt.ToArray();
                }
            }
        }

    }
}

