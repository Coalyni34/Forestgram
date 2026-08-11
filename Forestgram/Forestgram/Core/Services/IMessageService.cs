// ============================================================
// [EN] MESSAGE SERVICE INTERFACE
// [RU] ИНТЕРФЕЙС СЕРВИСА СООБЩЕНИЙ
// [ZH] 消息服务接口
// [FA] رابط سرویس پیام
// ============================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Forestgram.Core.Models;

namespace Forestgram.Core.Services
{
    // ============================================================
    // [EN] Interface for message operations
    // [RU] Интерфейс для операций с сообщениями
    // [ZH] 消息操作接口
    // [FA] رابط عملیات پیام
    // ============================================================
    public interface IMessageService
    {
        // ============================================================
        // [EN] Get messages from a chat
        // [RU] Получить сообщения из чата
        // [ZH] 从聊天中获取消息
        // [FA] دریافت پیام‌ها از یک چت
        // ============================================================
        /// <summary>
        /// [EN] Retrieves messages from a chat.
        /// [RU] Получает сообщения из чата.
        /// [ZH] 从聊天中获取消息。
        /// [FA] دریافت پیام‌ها از یک چت.
        /// </summary>
        /// <param name="chatId">[EN] Chat ID / [RU] ID чата / [ZH] 聊天ID / [FA] شناسه چت</param>
        /// <param name="limit">[EN] Number of messages to get / [RU] Количество сообщений / [ZH] 获取消息数量 / [FA] تعداد پیام‌ها</param>
        /// <returns>[EN] List of messages / [RU] Список сообщений / [ZH] 消息列表 / [FA] لیست پیام‌ها</returns>
        Task<IReadOnlyList<Models.Message>> GetMessagesAsync(long chatId, int limit = 50);

        // ============================================================
        // [EN] Send a text message to a chat
        // [RU] Отправить текстовое сообщение в чат
        // [ZH] 向聊天发送文本消息
        // [FA] ارسال پیام متنی به چت
        // ============================================================
        /// <summary>
        /// [EN] Sends a text message to a chat.
        /// [RU] Отправляет текстовое сообщение в чат.
        /// [ZH] 向聊天发送文本消息。
        /// [FA] ارسال پیام متنی به چت.
        /// </summary>
        /// <param name="chatId">[EN] Chat ID / [RU] ID чата / [ZH] 聊天ID / [FA] شناسه چت</param>
        /// <param name="text">[EN] Message text / [RU] Текст сообщения / [ZH] 消息文本 / [FA] متن پیام</param>
        /// <returns>[EN] True if sent successfully / [RU] True, если отправлено успешно / [ZH] 发送成功返回True / [FA] در صورت ارسال موفق True</returns>
        Task<bool> SendMessageAsync(long chatId, string text);

        // ============================================================
        // [EN] Subscribe to new messages
        // [RU] Подписаться на новые сообщения
        // [ZH] 订阅新消息
        // [FA] اشتراک در پیام‌های جدید
        // ============================================================
        /// <summary>
        /// [EN] Subscribes to new messages events.
        /// [RU] Подписывается на события новых сообщений.
        /// [ZH] 订阅新消息事件。
        /// [FA] اشتراک در رویدادهای پیام‌های جدید.
        /// </summary>
        /// <param name="handler">[EN] Event handler / [RU] Обработчик события / [ZH] 事件处理程序 / [FA] هندلر رویداد</param>
        void SubscribeToNewMessages(EventHandler<Models.Message> handler);

        // ============================================================
        // [EN] Unsubscribe from new messages
        // [RU] Отписаться от новых сообщений
        // [ZH] 取消订阅新消息
        // [FA] لغو اشتراک از پیام‌های جدید
        // ============================================================
        /// <summary>
        /// [EN] Unsubscribes from new messages events.
        /// [RU] Отписывается от событий новых сообщений.
        /// [ZH] 取消订阅新消息事件。
        /// [FA] لغو اشتراک از رویدادهای پیام‌های جدید.
        /// </summary>
        /// <param name="handler">[EN] Event handler / [RU] Обработчик события / [ZH] 事件处理程序 / [FA] هندلر رویداد</param>
        void UnsubscribeFromNewMessages(EventHandler<Models.Message> handler);
    }
}