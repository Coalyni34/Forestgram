// [EN] Message model: text, sender, date, outgoing status
// [RU] Модель сообщения: текст, отправитель, дата, статус исходящего
// [ZH] 消息模型：文本、发送者、日期、发送状态
// [FA] مدل پیام: متن، فرستنده، تاریخ، وضعیت خروجی

using System;

namespace Forestgram.Core.Models
{
    public class Message
    {
        public long Id { get; set; }
        public long ChatId { get; set; }
        public User? Sender { get; set; }
        public string? Text { get; set; }
        public DateTime Date { get; set; }
        public bool IsOutgoing { get; set; }
    }
}