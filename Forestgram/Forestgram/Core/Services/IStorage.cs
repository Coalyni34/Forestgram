using System.Threading.Tasks;

namespace Forestgram.Core.Services
{
    public interface IStorage
    {
        Task SaveSessionAsync(byte[] sessionData);
        Task<byte[]?> LoadSessionAsync();
        Task SaveSettingAsync(string key, string value);
        Task<string?> LoadSettingAsync(string key);
    }
}