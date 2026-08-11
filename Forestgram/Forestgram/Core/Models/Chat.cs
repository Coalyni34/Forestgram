// [EN] Chat model: private, group, supergroup, channel
// [RU] Модель чата: личный, группа, супергруппа, канал
// [ZH] 聊天模型：私聊、群组、超级群组、频道
// [FA] مدل چت: خصوصی، گروه، سوپرگروه، کانال

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
        Private,      // [EN] Private chat / [RU] Личный чат / [ZH] 私聊 / [FA] چت خصوصی
        Group,        // [EN] Group / [RU] Группа / [ZH] 群组 / [FA] گروه
        SuperGroup,   // [EN] Supergroup / [RU] Супергруппа / [ZH] 超级群组 / [FA] سوپرگروه
        Channel       // [EN] Channel / [RU] Канал / [ZH] 频道 / [FA] کانال
    }
}