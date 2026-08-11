using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forestgram.Core.Models;

namespace Forestgram.Core.Services
{
    public interface IMessageService
    {
        Task<IReadOnlyList<Models.Message>> GetMessagesAsync(long chatId, int limit = 50);
        Task<bool> SendMessageAsync(long chatId, string text);
        void SubscribeToNewMessages(EventHandler<Models.Message> handler);
        void UnsubscribeFromNewMessages(EventHandler<Models.Message> handler);
    }
}