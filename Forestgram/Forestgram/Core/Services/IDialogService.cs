// ============================================================
// [EN] DIALOG SERVICE INTERFACE
// [RU] ИНТЕРФЕЙС СЕРВИСА ДИАЛОГОВ
// [ZH] 对话服务接口
// [FA] رابط سرویس گفتگوها
// ============================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Forestgram.Core.Models;

namespace Forestgram.Core.Services
{
    // ============================================================
    // [EN] Interface for dialog (chat list) operations with caching
    // [RU] Интерфейс для операций с диалогами (список чатов) с кэшированием
    // [ZH] 对话（聊天列表）操作接口，带缓存
    // [FA] رابط عملیات گفتگوها (لیست چت‌ها) با کش
    // ============================================================
    public interface IDialogService
    {
        // ============================================================
        // [EN] Get all dialogs (chats) from cache or Telegram
        // [RU] Получить все диалоги (чаты) из кэша или Telegram
        // [ZH] 从缓存或Telegram获取所有对话（聊天）
        // [FA] دریافت همه گفتگوها (چت‌ها) از کش یا تلگرام
        // ============================================================
        /// <summary>
        /// [EN] Retrieves all dialogs. Uses cache unless forceRefresh is true.
        /// [RU] Получает все диалоги. Использует кэш, если forceRefresh = false.
        /// [ZH] 获取所有对话。如果forceRefresh为false则使用缓存。
        /// [FA] دریافت همه گفتگوها. در صورت نبود forceRefresh از کش استفاده می‌کند.
        /// </summary>
        /// <param name="forceRefresh">
        /// [EN] If true, ignores cache and fetches from Telegram
        /// [RU] Если true, игнорирует кэш и загружает из Telegram
        /// [ZH] 如果为true，忽略缓存并从Telegram获取
        /// [FA] اگر true باشد، کش را نادیده گرفته و از تلگرام دریافت می‌کند
        /// </param>
        /// <returns>[EN] List of chats / [RU] Список чатов / [ZH] 聊天列表 / [FA] لیست چت‌ها</returns>
        Task<IReadOnlyList<Models.Chat>> GetDialogsAsync(bool forceRefresh = false);

        // ============================================================
        // [EN] Get a specific dialog by its ID
        // [RU] Получить конкретный диалог по его ID
        // [ZH] 通过ID获取特定对话
        // [FA] دریافت یک گفتگوی خاص با شناسه آن
        // ============================================================
        /// <summary>
        /// [EN] Finds and returns a chat by its ID.
        /// [RU] Находит и возвращает чат по его ID.
        /// [ZH] 通过ID查找并返回聊天。
        /// [FA] چت را با شناسه آن پیدا کرده و برمی‌گرداند.
        /// </summary>
        /// <param name="id">[EN] Chat ID / [RU] ID чата / [ZH] 聊天ID / [FA] شناسه چت</param>
        /// <returns>[EN] Chat object or null if not found / [RU] Объект чата или null, если не найден / [ZH] 聊天对象，如果未找到则为null / [FA] شیء چت یا null در صورت پیدا نشدن</returns>
        Task<Models.Chat?> GetDialogByIdAsync(long id);

        // ============================================================
        // [EN] Clear the cache
        // [RU] Очистить кэш
        // [ZH] 清除缓存
        // [FA] پاک کردن کش
        // ============================================================
        /// <summary>
        /// [EN] Clears the internal cache of dialogs.
        /// [RU] Очищает внутренний кэш диалогов.
        /// [ZH] 清除对话的内部缓存。
        /// [FA] کش داخلی گفتگوها را پاک می‌کند.
        /// </summary>
        void ClearCache();
    }
}