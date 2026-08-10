using System;

namespace Forestgram.Core.Models
{
    public class Chat
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public ChatType Type { get; set; }
        public User? LastMessageSender { get; set; }
        public string? LastMessageText { get; set; }
        public DateTime? LastMessageDate { get; set; }
    }
    public enum ChatType
    {
        Private,
        Group,
        SuperGroup,
        Channel
    }
}