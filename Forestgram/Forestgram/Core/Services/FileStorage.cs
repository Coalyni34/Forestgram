using System;
using System.IO;
using System.Threading.Tasks;
using Forestgram.Core.Services;

namespace Forestgram.Core.Services
{
    public class FileStorage : IStorage
    {
        private readonly string _basePath;

        public FileStorage()
        {
            _basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Forestgram");
            Directory.CreateDirectory(_basePath);
        }

        public async Task SaveSessionAsync(byte[] sessionData)
        {
            await File.WriteAllBytesAsync(Path.Combine(_basePath, "session.dat"), sessionData);
        }

        public async Task<byte[]?> LoadSessionAsync()
        {
            var path = Path.Combine(_basePath, "session.dat");
            return File.Exists(path) ? await File.ReadAllBytesAsync(path) : null;
        }

        public async Task SaveSettingAsync(string key, string value)
        {
            await File.WriteAllTextAsync(Path.Combine(_basePath, $"{key}.txt"), value);
        }

        public async Task<string?> LoadSettingAsync(string key)
        {
            var path = Path.Combine(_basePath, $"{key}.txt");
            return File.Exists(path) ? await File.ReadAllTextAsync(path) : null;
        }
    }
}