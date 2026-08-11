using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forestgram.Core.Models;

namespace Forestgram.Core.Services
{
    public class MessageService : IMessageService, IDisposable
    {
        private readonly ITelegramClient _client;
        private bool _disposed;

        public MessageService(ITelegramClient client)
        {
            _client = client;
        }

        public async Task<IReadOnlyList<Models.Message>> GetMessagesAsync(long chatId, int limit = 50)
        {
            return await _client.GetMessagesAsync(chatId, limit);
        }

        public async Task<bool> SendMessageAsync(long chatId, string text)
        {
            return await _client.SendMessageAsync(chatId, text);
        }

        public void SubscribeToNewMessages(EventHandler<Models.Message> handler)
        {
            _client.NewMessage += handler;
        }

        public void UnsubscribeFromNewMessages(EventHandler<Models.Message> handler)
        {
            _client.NewMessage -= handler;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}