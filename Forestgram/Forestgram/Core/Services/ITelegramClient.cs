using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forestgram.Core.Models;

namespace Forestgram.Core.Services
{
    public interface ITelegramClient : IDisposable
    {
        // Авторизация
        Task<bool> LoginAsync(string phoneNumber, Func<string, string> codeProvider);

        // Данные пользователя
        Task<User> GetCurrentUserAsync();

        // Чаты
        Task<IReadOnlyList<Chat>> GetChatsAsync();

        // Сообщения
        Task<IReadOnlyList<Models.Message>> GetMessagesAsync(long chatId, int limit = 50);
        Task<bool> SendMessageAsync(long chatId, string text);

        // События обновлений (подписка)
        event EventHandler<Message>? NewMessage;
        event EventHandler<Chat>? ChatUpdated;

        // Проверка соединения
        bool IsConnected { get; }   
    }
}