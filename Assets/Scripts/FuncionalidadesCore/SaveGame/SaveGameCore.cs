using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Core del sistema de guardado/carga.
    /// Lógica pura de serialización JSON usando JsonUtility de Unity (sin Newtonsoft.Json).
    /// </summary>
    public class SaveGameCore
    {
        private readonly string saveFolderPath;
        private readonly string encryptionKey;
        private readonly bool useEncryption;

        public SaveGameCore(string saveFolderPath, string encryptionKey = null, bool useEncryption = false)
        {
            this.saveFolderPath = saveFolderPath;
            this.encryptionKey = encryptionKey;
            this.useEncryption = useEncryption && !string.IsNullOrEmpty(encryptionKey);

            if (!Directory.Exists(saveFolderPath))
                Directory.CreateDirectory(saveFolderPath);
        }

        /// <summary>Guardar dato como JSON en un archivo. El objeto debe ser serializable por JsonUtility.</summary>
        public async Task SaveAsync<T>(string fileName, T data)
        {
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(saveFolderPath, fileName);

            if (useEncryption)
                await EncryptAndWriteAsync(path, json);
            else
                await File.WriteAllTextAsync(path, json);
        }

        /// <summary>Guardar un string JSON directamente.</summary>
        public async Task SaveRawAsync(string fileName, string json)
        {
            string path = Path.Combine(saveFolderPath, fileName);

            if (useEncryption)
                await EncryptAndWriteAsync(path, json);
            else
                await File.WriteAllTextAsync(path, json);
        }

        /// <summary>Cargar dato desde JSON.</summary>
        public async Task<T> LoadAsync<T>(string fileName)
        {
            string path = Path.Combine(saveFolderPath, fileName);
            if (!File.Exists(path)) return default;

            string json;
            if (useEncryption)
                json = await ReadAndDecryptAsync(path);
            else
                json = await File.ReadAllTextAsync(path);

            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>Cargar el JSON crudo como string.</summary>
        public async Task<string> LoadRawAsync(string fileName)
        {
            string path = Path.Combine(saveFolderPath, fileName);
            if (!File.Exists(path)) return null;

            if (useEncryption)
                return await ReadAndDecryptAsync(path);
            else
                return await File.ReadAllTextAsync(path);
        }

        /// <summary>Verificar si un archivo de guardado existe.</summary>
        public bool SaveExists(string fileName)
        {
            return File.Exists(Path.Combine(saveFolderPath, fileName));
        }

        /// <summary>Eliminar un archivo de guardado.</summary>
        public bool DeleteSave(string fileName)
        {
            string path = Path.Combine(saveFolderPath, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }

        /// <summary>Obtener lista de archivos de guardado.</summary>
        public string[] GetSaveFiles(string extension = ".json")
        {
            return Directory.GetFiles(saveFolderPath, $"*{extension}");
        }

        // --- Encriptación AES ---

        private async Task EncryptAndWriteAsync(string path, string text)
        {
            byte[] iv = new byte[16];
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using MemoryStream ms = new();
            using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
            using (StreamWriter sw = new(cs))
            {
                await sw.WriteAsync(text);
            }

            await File.WriteAllBytesAsync(path, ms.ToArray());
        }

        private async Task<string> ReadAndDecryptAsync(string path)
        {
            byte[] buffer = await File.ReadAllBytesAsync(path);
            byte[] iv = new byte[16];

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream ms = new(buffer);
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new(cs);

            return await sr.ReadToEndAsync();
        }
    }
}
