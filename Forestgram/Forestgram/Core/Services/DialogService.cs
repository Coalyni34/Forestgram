// ============================================================
// [EN] DIALOG SERVICE - Caches and retrieves chat lists
// [RU] СЕРВИС ДИАЛОГОВ - Кэширует и получает списки чатов
// [ZH] 对话服务 - 缓存和获取聊天列表
// [FA] سرویس گفتگوها - کش و دریافت لیست چت‌ها
// ============================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forestgram.Core.Models;

namespace Forestgram.Core.Services
{
    // ============================================================
    // [EN] Dialog service implementation with caching
    // [RU] Реализация сервиса диалогов с кэшированием
    // [ZH] 带缓存的对话服务实现
    // [FA] پیاده‌سازی سرویس گفتگوها با کش
    // ============================================================
    public class DialogService : IDialogService
    {
        // [EN] Telegram client instance
        // [RU] Экземпляр клиента Telegram
        // [ZH] Telegram客户端实例
        // [FA] نمونه کلاینت تلگرام
        private readonly ITelegramClient _client;

        // [EN] Cached dialogs (null means no cache)
        // [RU] Кэшированные диалоги (null означает отсутствие кэша)
        // [ZH] 缓存的对话（null表示无缓存）
        // [FA] گفتگوهای کش شده (null به معنای عدم وجود کش)
        private IReadOnlyList<Models.Chat>? _cachedDialogs;

        // ============================================================
        // [EN] Constructor - initialize with Telegram client
        // [RU] Конструктор - инициализация с клиентом Telegram
        // [ZH] 构造函数 - 使用Telegram客户端初始化
        // [FA] سازنده - راه‌اندازی با کلاینت تلگرام
        // ============================================================
        public DialogService(ITelegramClient client)
        {
            _client = client;
        }

        // ============================================================
        // [EN] Get all dialogs (with optional cache)
        // [RU] Получить все диалоги (с опциональным кэшем)
        // [ZH] 获取所有对话（带可选缓存）
        // [FA] دریافت همه گفتگوها (با کش اختیاری)
        // ============================================================
        public async Task<IReadOnlyList<Models.Chat>> GetDialogsAsync(bool forceRefresh = false)
        {
            // [EN] Return cached dialogs if available and refresh is not forced
            // [RU] Возвращаем кэшированные диалоги, если они есть и не принудительное обновление
            // [ZH] 如果缓存可用且未强制刷新，则返回缓存的对话
            // [FA] در صورت وجود کش و عدم اجبار به بروزرسانی، گفتگوهای کش شده را برگردان
            if (_cachedDialogs != null && !forceRefresh)
                return _cachedDialogs;

            // [EN] Fetch fresh dialogs from Telegram and update cache
            // [RU] Загружаем свежие диалоги из Telegram и обновляем кэш
            // [ZH] 从Telegram获取新对话并更新缓存
            // [FA] دریافت گفتگوهای جدید از تلگرام و بروزرسانی کش
            _cachedDialogs = await _client.GetChatsAsync();
            return _cachedDialogs;
        }

        // ============================================================
        // [EN] Get a specific dialog by ID
        // [RU] Получить конкретный диалог по ID
        // [ZH] 通过ID获取特定对话
        // [FA] دریافت یک گفتگوی خاص با شناسه
        // ============================================================
        public async Task<Models.Chat?> GetDialogByIdAsync(long id)
        {
            // [EN] Get all dialogs (uses cache if available)
            // [RU] Получаем все диалоги (использует кэш, если есть)
            // [ZH] 获取所有对话（如果可用则使用缓存）
            // [FA] دریافت همه گفتگوها (در صورت وجود از کش استفاده می‌کند)
            var dialogs = await GetDialogsAsync();

            // [EN] Find and return chat with matching ID
            // [RU] Находим и возвращаем чат с соответствующим ID
            // [ZH] 查找并返回匹配ID的聊天
            // [FA] پیدا کردن و برگرداندن چت با شناسه منطبق
            return dialogs.FirstOrDefault(c => c.Id == id);
        }

        // ============================================================
        // [EN] Clear cache (force next call to fetch from Telegram)
        // [RU] Очистить кэш (принудительно загрузить из Telegram при следующем вызове)
        // [ZH] 清除缓存（强制下次调用从Telegram获取）
        // [FA] پاک کردن کش (اجبار به دریافت از تلگرام در فراخوانی بعدی)
        // ============================================================
        public void ClearCache() => _cachedDialogs = null;
    }
}