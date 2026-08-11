// [EN] Main Telegram client interface: login, chats, messages, updates
// [RU] Основной интерфейс клиента Telegram: вход, чаты, сообщения, обновления
// [ZH] Telegram客户端主接口：登录、聊天、消息、更新
// [FA] رابط اصلی کلاینت تلگرام: ورود، چت‌ها، پیام‌ها، بروزرسانی‌ها

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forestgram.Core.Models;

namespace Forestgram.Core.Services
{
    public interface ITelegramClient : IDisposable
    {
        Task<bool> LoginAsync(string phoneNumber, Func<string, string> codeProvider);
        Task<User> GetCurrentUserAsync();
        Task<IReadOnlyList<Chat>> GetChatsAsync();
        Task<IReadOnlyList<Models.Message>> GetMessagesAsync(long chatId, int limit = 50);
        Task<bool> SendMessageAsync(long chatId, string text);
        event EventHandler<Message>? NewMessage;
        event EventHandler<Chat>? ChatUpdated;
        bool IsConnected { get; }
    }
}