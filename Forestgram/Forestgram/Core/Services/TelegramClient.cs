// [EN] TelegramClient implementation using WTelegramClient library
// [RU] Реализация TelegramClient с использованием библиотеки WTelegramClient
// [ZH] 使用WTelegramClient库实现Telegram客户端
// [FA] پیاده‌سازی کلاینت تلگرام با استفاده از کتابخانه WTelegramClient

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Forestgram.Core.Models;
using TL;

namespace Forestgram.Core.Services
{
    // [EN] Main Telegram client class
    // [RU] Основной класс клиента Telegram
    // [ZH] Telegram客户端主类
    // [FA] کلاس اصلی کلاینت تلگرام
    public class TelegramClient : ITelegramClient
    {
        // ============================================================
        // [EN] Private fields
        // [RU] Приватные поля
        // [ZH] 私有字段
        // [FA] فیلدهای خصوصی
        // ============================================================
        private WTelegram.Client? _client; // [EN] WTelegram client instance / [RU] Экземпляр клиента WTelegram / [ZH] WTelegram客户端实例 / [FA] نمونه کلاینت WTelegram
        private TL.User? _currentUser; // [EN] Currently logged-in user / [RU] Текущий пользователь / [ZH] 当前登录用户 / [FA] کاربر فعلی وارد شده
        private readonly IStorage _storage; // [EN] Storage service / [RU] Сервис хранения / [ZH] 存储服务 / [FA] سرویس ذخیره‌سازی
        private readonly string _apiId; // [EN] Telegram API ID / [RU] API ID Telegram / [ZH] Telegram API ID / [FA] شناسه API تلگرام
        private readonly string _apiHash; // [EN] Telegram API Hash / [RU] API Hash Telegram / [ZH] Telegram API Hash / [FA] هش API تلگرام
        private readonly string _sessionPath; // [EN] Path to session file / [RU] Путь к файлу сессии / [ZH] 会话文件路径 / [FA] مسیر فایل نشست
        private bool _disposed; // [EN] Disposal flag / [RU] Флаг удаления / [ZH] 释放标记 / [FA] پرچم آزادسازی

        private string? _phoneNumber; // [EN] Phone number for login / [RU] Номер телефона для входа / [ZH] 登录电话号码 / [FA] شماره تلفن برای ورود
        private Func<string, string>? _codeProvider; // [EN] Code/Password provider delegate / [RU] Делегат для получения кода/пароля / [ZH] 验证码/密码提供者委托 / [FA] نماینده دریافت کد/رمز عبور

        // ============================================================
        // [EN] Public events for UI updates
        // [RU] Публичные события для обновления UI
        // [ZH] 用于UI更新的公共事件
        // [FA] رویدادهای عمومی برای بروزرسانی UI
        // ============================================================
        public event EventHandler<Models.Message>? NewMessage; // [EN] Fired when new message arrives / [RU] Срабатывает при новом сообщении / [ZH] 收到新消息时触发 / [FA] هنگام دریافت پیام جدید فعال می‌شود
        public event EventHandler<Models.Chat>? ChatUpdated; // [EN] Fired when chat is updated / [RU] Срабатывает при обновлении чата / [ZH] 聊天更新时触发 / [FA] هنگام بروزرسانی چت فعال می‌شود

        // [EN] Connection status / [RU] Статус соединения / [ZH] 连接状态 / [FA] وضعیت اتصال
        public bool IsConnected => _client != null && _currentUser != null;

        // ============================================================
        // [EN] Constructor: initialize client and session path
        // [RU] Конструктор: инициализация клиента и пути к сессии
        // [ZH] 构造函数：初始化客户端和会话路径
        // [FA] سازنده: راه‌اندازی کلاینت و مسیر نشست
        // ============================================================
        public TelegramClient(IStorage storage, string apiId, string apiHash)
        {
            _storage = storage;
            _apiId = apiId;
            _apiHash = apiHash;
            _sessionPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Forestgram",
            "session.dat");

            var dir = Path.GetDirectoryName(_sessionPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        // ============================================================
        // [EN] CONFIGURATION - Called by WTelegramClient to get data
        // [RU] КОНФИГУРАЦИЯ - Вызывается WTelegramClient для получения данных
        // [ZH] 配置 - 由WTelegramClient调用以获取数据
        // [FA] پیکربندی - توسط WTelegramClient برای دریافت داده‌ها فراخوانی می‌شود
        // ============================================================
        private string? Config(string what)
        {
            return what switch
            {
                "api_id" => _apiId, // [EN] API ID / [RU] API ID / [ZH] API ID / [FA] شناسه API
                "api_hash" => _apiHash, // [EN] API Hash / [RU] API Hash / [ZH] API Hash / [FA] هش API
                "session_pathname" => _sessionPath, // [EN] Session file path / [RU] Путь к файлу сессии / [ZH] 会话文件路径 / [FA] مسیر فایل نشست
                "phone_number" => _phoneNumber, // [EN] Phone number / [RU] Номер телефона / [ZH] 电话号码 / [FA] شماره تلفن
                "verification_code" => _codeProvider?.Invoke("verification_code"), // [EN] Get verification code / [RU] Получить код подтверждения / [ZH] 获取验证码 / [FA] دریافت کد تایید
                "password" => _codeProvider?.Invoke("password"), // [EN] Get 2FA password / [RU] Получить пароль 2FA / [ZH] 获取两步验证密码 / [FA] دریافت رمز تایید دو مرحله‌ای
                _ => null
            };
        }

        // ============================================================
        // [EN] AUTHORIZATION - Login to Telegram
        // [RU] АВТОРИЗАЦИЯ - Вход в Telegram
        // [ZH] 授权 - 登录Telegram
        // [FA] احراز هویت - ورود به تلگرام
        // ============================================================
        public async Task<bool> LoginAsync(string phoneNumber, Func<string, string> codeProvider)
        {
            try
            {
                // [EN] Store phone number and code provider
                // [RU] Сохраняем номер телефона и провайдер кода
                // [ZH] 存储电话号码和验证码提供者
                // [FA] ذخیره شماره تلفن و ارائه‌دهنده کد
                _phoneNumber = phoneNumber;
                _codeProvider = codeProvider;

                // [EN] Create WTelegram client with config delegate
                // [RU] Создаём клиент WTelegram с делегатом конфигурации
                // [ZH] 使用配置委托创建WTelegram客户端
                // [FA] ایجاد کلاینت WTelegram با نماینده پیکربندی
                _client = new WTelegram.Client(Config);
                _client.OnUpdates += HandleUpdate;

                // [EN] Code settings (empty for default)
                // [RU] Настройки кода (пустые, по умолчанию)
                // [ZH] 验证码设置（默认空）
                // [FA] تنظیمات کد (پیش‌فرض خالی)
                var codeSettings = new CodeSettings
                {

                };

                // [EN] Login (session auto-saved to _sessionPath)
                // [RU] Вход (сессия автоматически сохраняется в _sessionPath)
                // [ZH] 登录（会话自动保存到_sessionPath）
                // [FA] ورود (نشست به طور خودکار در _sessionPath ذخیره می‌شود)
                var user = await _client.LoginUserIfNeeded(
                    settings: codeSettings,
                    reloginOnFailedResume: true
                );

                _currentUser = user;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TelegramClient] Login error: {ex.Message}");
                return false;
            }
        }

        // [EN] Get current logged-in user
        // [RU] Получить текущего пользователя
        // [ZH] 获取当前登录用户
        // [FA] دریافت کاربر فعلی وارد شده
        public async Task<Models.User> GetCurrentUserAsync()
        {
            if (!IsConnected || _currentUser == null)
            {
                throw new InvalidOperationException("Not logged in");
            }

            return MapUser(_currentUser);
        }

        // ============================================================
        // [EN] CHATS - Get all chats/dialogs
        // [RU] ЧАТЫ - Получить все чаты/диалоги
        // [ZH] 聊天 - 获取所有聊天/对话
        // [FA] چت‌ها - دریافت همه چت‌ها/گفتگوها
        // ============================================================
        public async Task<IReadOnlyList<Models.Chat>> GetChatsAsync()
        {
            if (_client == null)
                throw new InvalidOperationException("Client not initialized");

            try
            {
                // [EN] Get all dialogs from Telegram
                // [RU] Получаем все диалоги из Telegram
                // [ZH] 从Telegram获取所有对话
                // [FA] دریافت همه گفتگوها از تلگرام
                var dialogs = await _client.Messages_GetAllDialogs();
                var chats = new List<Models.Chat>();

                // [EN] Map groups and channels (ChatBase)
                // [RU] Маппим группы и каналы (ChatBase)
                // [ZH] 映射群组和频道（ChatBase）
                // [FA] نگاشت گروه‌ها و کانال‌ها (ChatBase)
                foreach (var chat in dialogs.chats.Values)
                {
                    var mapped = MapChat(chat);
                    if (mapped != null)
                        chats.Add(mapped);
                }

                // [EN] Map private chats (User)
                // [RU] Маппим личные чаты (User)
                // [ZH] 映射私聊（User）
                // [FA] نگاشت چت‌های خصوصی (User)
                foreach (var user in dialogs.users.Values)
                {
                    var mapped = MapUserToChat(user);
                    if (mapped != null)
                        chats.Add(mapped);
                }

                return chats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TelegramClient] GetChatsAsync error: {ex.Message}");
                return Array.Empty<Models.Chat>();
            }
        }

        // ============================================================
        // [EN] MESSAGES - Get chat message history
        // [RU] СООБЩЕНИЯ - Получить историю сообщений чата
        // [ZH] 消息 - 获取聊天消息历史
        // [FA] پیام‌ها - دریافت تاریخچه پیام‌های چت
        // ============================================================
        public async Task<IReadOnlyList<Models.Message>> GetMessagesAsync(long chatId, int limit = 50)
        {
            if (_client == null)
            {
                throw new InvalidOperationException("Client not initialized");
            }

            try
            {
                var dialogs = await _client.Messages_GetAllDialogs();

                // [EN] Find chat in groups/channels
                // [RU] Ищем чат в группах/каналах
                // [ZH] 在群组/频道中查找聊天
                // [FA] جستجوی چت در گروه‌ها/کانال‌ها
                if (dialogs.chats.TryGetValue(chatId, out var chat))
                {
                    var history = await _client.Messages_GetHistory(chat, limit: 50);

                    var result = new List<Models.Message>();

                    foreach (var msg in history.Messages)
                    {
                        var mapped = MapMessage(msg, chatId);
                        if (mapped != null)
                        {
                            result.Add(mapped);
                        }
                    }

                    return result;
                }
                else
                {
                    return Array.Empty<Models.Message>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TelegramClient] GetMessageAsync error: {ex.Message}");

                return Array.Empty<Models.Message>();
            }
        }

        // ============================================================
        // [EN] SEND MESSAGE - Send text message to chat
        // [RU] ОТПРАВКА СООБЩЕНИЙ - Отправить текстовое сообщение в чат
        // [ZH] 发送消息 - 向聊天发送文本消息
        // [FA] ارسال پیام - ارسال پیام متنی به چت
        // ============================================================
        public async Task<bool> SendMessageAsync(long chatId, string text)
        {
            if (_client == null)
            {
                throw new InvalidOperationException("Client not initialized");
            }

            try
            {
                var dialogs = await _client.Messages_GetAllDialogs();

                // [EN] Find chat in groups/channels
                // [RU] Ищем чат в группах/каналах
                // [ZH] 在群组/频道中查找聊天
                // [FA] جستجوی چت در گروه‌ها/کانال‌ها
                if (dialogs.chats.TryGetValue(chatId, out var chat))
                {
                    var peer = chat;
                    await _client.SendMessageAsync(peer, text);
                    return true;
                }
                // [EN] Find user in private chats
                // [RU] Ищем пользователя в личных чатах
                // [ZH] 在私聊中查找用户
                // [FA] جستجوی کاربر در چت‌های خصوصی
                else if (dialogs.users.TryGetValue(chatId, out var user))
                {
                    var peer = user;
                    await _client.SendMessageAsync(peer, text);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TelegramClient] SendMessageAsync error: {ex.Message}");
                return false;
            }
        }

        // ============================================================
        // [EN] UPDATE HANDLER - Process incoming updates from Telegram
        // [RU] ОБРАБОТКА ОБНОВЛЕНИЙ - Обработка входящих обновлений от Telegram
        // [ZH] 更新处理 - 处理来自Telegram的传入更新
        // [FA] مدیریت بروزرسانی - پردازش بروزرسانی‌های دریافتی از تلگرام
        // ============================================================
        private async Task HandleUpdate(UpdatesBase updates)
        {
            try
            {
                foreach (var upd in updates.UpdateList)
                {
                    switch (upd)
                    {
                        // [EN] New message received
                        // [RU] Получено новое сообщение
                        // [ZH] 收到新消息
                        // [FA] پیام جدید دریافت شد
                        case UpdateNewMessage unm:
                            var msg = MapMessage(unm.message, unm.message.Peer.ID);
                            if (msg != null)
                            {
                                NewMessage?.Invoke(this, msg);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TelegramClient] HandleUpdate error: {ex.Message}");
            }
        }

        // ============================================================
        // [EN] MAPPERS - Convert TL types to Core models
        // [RU] МАППЕРЫ - Преобразование TL-типов в модели Core
        // [ZH] 映射器 - 将TL类型转换为Core模型
        // [FA] نگاشت‌گرها - تبدیل انواع TL به مدل‌های Core
        // ============================================================

        // [EN] Map TL.User to Core User model
        // [RU] Маппинг TL.User в модель Core User
        // [ZH] 将TL.User映射到Core User模型
        // [FA] نگاشت TL.User به مدل Core User
        private static Models.User MapUser(TL.User user) =>
        new()
        {
            Id = user.id,
            FirstName = user.first_name,
            LastName = user.last_name,
            Username = user.username,
            PhoneNumber = user.phone
        };

        // [EN] Map TL.ChatBase to Core Chat model (groups, channels)
        // [RU] Маппинг TL.ChatBase в модель Core Chat (группы, каналы)
        // [ZH] 将TL.ChatBase映射到Core Chat模型（群组、频道）
        // [FA] نگاشت TL.ChatBase به مدل Core Chat (گروه‌ها، کانال‌ها)
        private static Models.Chat? MapChat(TL.ChatBase chatBase)
        {
            // [EN] Regular group
            // [RU] Обычная группа
            // [ZH] 普通群组
            // [FA] گروه معمولی
            if (chatBase is TL.Chat chat)
            {
                return new Models.Chat
                {
                    Id = chat.id,
                    Title = chat.title,
                    Type = ChatType.Group
                };
            }

            // [EN] Channel or Supergroup (check broadcast flag)
            // [RU] Канал или Супергруппа (проверяем флаг broadcast)
            // [ZH] 频道或超级群组（检查广播标志）
            // [FA] کانال یا سوپرگروه (بررسی پرچم broadcast)
            if (chatBase is TL.Channel channel)
            {
                var isBroadcast = (channel.flags & TL.Channel.Flags.broadcast) != 0;
                var chatType = isBroadcast ? ChatType.Channel : ChatType.SuperGroup;

                return new Models.Chat
                {
                    Id = channel.id,
                    Title = channel.title,
                    Type = chatType
                };
            }

            return null;
        }

        // [EN] Map TL.User to Core Chat model (private chat)
        // [RU] Маппинг TL.User в модель Core Chat (личный чат)
        // [ZH] 将TL.User映射到Core Chat模型（私聊）
        // [FA] نگاشت TL.User به مدل Core Chat (چت خصوصی)
        private static Models.Chat MapUserToChat(TL.User user)
        {
            var title = $"{user.first_name} {user.last_name}".Trim();
            if (string.IsNullOrEmpty(title))
                title = user.username ?? user.phone ?? "Unknown";

            return new Models.Chat
            {
                Id = user.id,
                Title = title,
                Type = ChatType.Private
            };
        }

        // [EN] Map TL.MessageBase to Core Message model
        // [RU] Маппинг TL.MessageBase в модель Core Message
        // [ZH] 将TL.MessageBase映射到Core Message模型
        // [FA] نگاشت TL.MessageBase به مدل Core Message
        private static Models.Message? MapMessage(TL.MessageBase msgBase, long chatId)
        {
            if (msgBase is not TL.Message msg)
            {
                return null;
            }

            return new Models.Message
            {
                Id = msg.id,
                ChatId = chatId,
                Text = msg.message ?? string.Empty,
                Date = msg.date,
                Sender = msg.from_id?.ID != null
                ? new Models.User { Id = msg.from_id.ID }
                : null
            };
        }

        // ============================================================
        // [EN] DISPOSE - Release resources
        // [RU] DISPOSE - Освобождение ресурсов
        // [ZH] 释放 - 释放资源
        // [FA] آزادسازی - آزادسازی منابع
        // ============================================================
        public void Dispose()
        {
            if (_disposed) return;

            if (_client != null)
            {
                _client.OnUpdates -= HandleUpdate; // [EN] Unsubscribe / [RU] Отписка / [ZH] 取消订阅 / [FA] لغو اشتراک
                _client.Dispose();
            }

            _disposed = true;
        }
    }
}