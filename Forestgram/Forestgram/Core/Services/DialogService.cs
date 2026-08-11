using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forestgram.Core.Models;

namespace Forestgram.Core.Services
{
    public class DialogService : IDialogService
    {
        private readonly ITelegramClient _client;
        private IReadOnlyList<Models.Chat>? _cachedDialogs;

        public DialogService(ITelegramClient client)
        {
            _client = client;
        }

        public async Task<IReadOnlyList<Models.Chat>> GetDialogsAsync(bool forceRefresh = false)
        {
            if (_cachedDialogs != null && !forceRefresh)
                return _cachedDialogs;

            _cachedDialogs = await _client.GetChatsAsync();
            return _cachedDialogs;
        }

        public async Task<Models.Chat?> GetDialogByIdAsync(long id)
        {
            var dialogs = await GetDialogsAsync();
            return dialogs.FirstOrDefault(c => c.Id == id);
        }

        public void ClearCache() => _cachedDialogs = null;
    }
}