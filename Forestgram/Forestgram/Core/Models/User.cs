// [EN] User model representing a Telegram user
// [RU] Модель пользователя Telegram
// [ZH] 表示Telegram用户的用户模型
// [FA] مدل کاربر تلگرام

namespace Forestgram.Core.Models
{
    public class User
    {
        public long Id { set; get; }
        public string? FirstName { set; get; }
        public string? LastName { set; get; }
        public string? Username { set; get; }
        public string? PhoneNumber { set; get; }
    }
}