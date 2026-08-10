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