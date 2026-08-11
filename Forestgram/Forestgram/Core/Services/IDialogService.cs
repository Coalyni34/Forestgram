using System.Collections.Generic;
using System.Threading.Tasks;
using Forestgram.Core.Models;

namespace Forestgram.Core.Services
{
    public interface IDialogService
    {
        Task<IReadOnlyList<Models.Chat>> GetDialogsAsync(bool forceRefresh = false);
        Task<Models.Chat?> GetDialogByIdAsync(long id);
        void ClearCache();
    }
}