// ============================================================
// [EN] MESSAGE SERVICE - Handles messages, subscription to new messages
// [RU] СЕРВИС СООБЩЕНИЙ - Обработка сообщений, подписка на новые
// [ZH] 消息服务 - 处理消息，订阅新消息
// [FA] سرویس پیام - مدیریت پیام‌ها، اشتراک پیام‌های جدید
// ============================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forestgram.Core.Models;

namespace Forestgram.Core.Services
{
    // ============================================================
    // [EN] Message service implementation
    // [RU] Реализация сервиса сообщений
    // [ZH] 消息服务实现
    // [FA] پیاده‌سازی سرویس پیام
    // ============================================================
    public class MessageService : IMessageService, IDisposable
    {
        // [EN] Telegram client instance
        // [RU] Экземпляр клиента Telegram
        // [ZH] Telegram客户端实例
        // [FA] نمونه کلاینت تلگرام
        private readonly ITelegramClient _client;
        
        // [EN] Disposal flag
        // [RU] Флаг удаления
        // [ZH] 释放标记
        // [FA] پرچم آزادسازی
        private bool _disposed;

        // ============================================================
        // [EN] Constructor - initialize with Telegram client
        // [RU] Конструктор - инициализация с клиентом Telegram
        // [ZH] 构造函数 - 使用Telegram客户端初始化
        // [FA] سازنده - راه‌اندازی با کلاینت تلگرام
        // ============================================================
        public MessageService(ITelegramClient client)
        {
            _client = client;
        }

        // ============================================================
        // [EN] Get messages from a chat
        // [RU] Получить сообщения из чата
        // [ZH] 从聊天中获取消息
        // [FA] دریافت پیام‌ها از یک چت
        // ============================================================
        public async Task<IReadOnlyList<Models.Message>> GetMessagesAsync(long chatId, int limit = 50)
        {
            // [EN] Delegate to Telegram client
            // [RU] Делегируем клиенту Telegram
            // [ZH] 委托给Telegram客户端
            // [FA] واگذاری به کلاینت تلگرام
            return await _client.GetMessagesAsync(chatId, limit);
        }

        // ============================================================
        // [EN] Send a text message to a chat
        // [RU] Отправить текстовое сообщение в чат
        // [ZH] 向聊天发送文本消息
        // [FA] ارسال پیام متنی به چت
        // ============================================================
        public async Task<bool> SendMessageAsync(long chatId, string text)
        {
            // [EN] Delegate to Telegram client
            // [RU] Делегируем клиенту Telegram
            // [ZH] 委托给Telegram客户端
            // [FA] واگذاری به کلاینت تلگرام
            return await _client.SendMessageAsync(chatId, text);
        }

        // ============================================================
        // [EN] Subscribe to new messages event
        // [RU] Подписаться на событие новых сообщений
        // [ZH] 订阅新消息事件
        // [FA] اشتراک در رویداد پیام‌های جدید
        // ============================================================
        public void SubscribeToNewMessages(EventHandler<Models.Message> handler)
        {
            // [EN] Attach handler to client's NewMessage event
            // [RU] Прикрепляем обработчик к событию NewMessage клиента
            // [ZH] 将处理程序附加到客户端的NewMessage事件
            // [FA] اتصال هندلر به رویداد NewMessage کلاینت
            _client.NewMessage += handler;
        }

        // ============================================================
        // [EN] Unsubscribe from new messages event
        // [RU] Отписаться от события новых сообщений
        // [ZH] 取消订阅新消息事件
        // [FA] لغو اشتراک از رویداد پیام‌های جدید
        // ============================================================
        public void UnsubscribeFromNewMessages(EventHandler<Models.Message> handler)
        {
            // [EN] Detach handler from client's NewMessage event
            // [RU] Открепляем обработчик от события NewMessage клиента
            // [ZH] 从客户端的NewMessage事件分离处理程序
            // [FA] جدا کردن هندلر از رویداد NewMessage کلاینت
            _client.NewMessage -= handler;
        }

        // ============================================================
        // [EN] Dispose resources
        // [RU] Освобождение ресурсов
        // [ZH] 释放资源
        // [FA] آزادسازی منابع
        // ============================================================
        public void Dispose()
        {
            if (_disposed) return;
            // [EN] This service doesn't own any disposable resources
            // [RU] Этот сервис не владеет ресурсами, требующими освобождения
            // [ZH] 此服务不拥有需要释放的资源
            // [FA] این سرویس منابع قابل آزادسازی ندارد
            _disposed = true;
        }
    }
}