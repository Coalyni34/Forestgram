// [EN] Storage interface: session and settings persistence
// [RU] Интерфейс хранилища: сохранение сессии и настроек
// [ZH] 存储接口：会话和设置的持久化
// [FA] رابط حافظه: ذخیره‌سازی نشست و تنظیمات

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