using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Forestgram.Core.Models;
using TL;

namespace Forestgram.Core.Services
{
    public class TelegramClient : ITelegramClient
    {
        private WTelegram.Client? _client;
        private TL.User? _currentUser;
        private readonly IStorage _storage;
        private readonly string _apiId;
        private readonly string _apiHash;
        private readonly string _sessionPath;
        private bool _disposed;

        private string? _phoneNumber;
        private Func<string, string>? _codeProvider;

        public event EventHandler<Models.Message>? NewMessage;
        public event EventHandler<Models.Chat>? ChatUpdated;

        public bool IsConnected => _client != null && _currentUser != null;

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
        //  КОНФИГУРАЦИЯ (вызывается WTelegramClient для получения данных)
        // ============================================================
        private string? Config(string what)
        {
            return what switch
            {
                "api_id" => _apiId,
                "api_hash" => _apiHash,
                "session_pathname" => _sessionPath,
                "phone_number" => _phoneNumber,
                "verification_code" => _codeProvider?.Invoke("verification_code"),
                "password" => _codeProvider?.Invoke("password"),
                _ => null
            };
        }

        // ============================================================
        //  АВТОРИЗАЦИЯ
        // ============================================================
        public async Task<bool> LoginAsync(string phoneNumber, Func<string, string> codeProvider)
        {
            try
            {
                _phoneNumber = phoneNumber;
                _codeProvider = codeProvider;


                // Создаем клиент с делегатом конфигурации
                _client = new WTelegram.Client(Config);
                _client.OnUpdates += HandleUpdate;

                // Настройки кода
                var codeSettings = new CodeSettings
                {

                };

                // Выполняем вход (сессия сохранится автоматически в _sessionPath)
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

        public async Task<Models.User> GetCurrentUserAsync()
        {
            if (!IsConnected || _currentUser == null)
            {
                throw new InvalidOperationException("Not logged in");
            }

            return MapUser(_currentUser);
        }

        // ============================================================
        //  ЧАТЫ
        // ============================================================
        public async Task<IReadOnlyList<Models.Chat>> GetChatsAsync()
        {
            if (_client == null)
                throw new InvalidOperationException("Client not initialized");

            try
            {
                var dialogs = await _client.Messages_GetAllDialogs();
                var chats = new List<Models.Chat>();

                foreach (var chat in dialogs.chats.Values)
                {
                    var mapped = MapChat(chat);
                    if (mapped != null)
                        chats.Add(mapped);
                }

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
        //  СООБЩЕНИЯ
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
        //  ОТПРАВКА СООБЩЕНИЙ
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

                if (dialogs.chats.TryGetValue(chatId, out var chat))
                {
                    var peer = chat;
                    await _client.SendMessageAsync(peer, text);
                    return true;
                }
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
        //  ОБРАБОТКА ОБНОВЛЕНИЙ
        // ============================================================        
        private async Task HandleUpdate(UpdatesBase updates)
        {
            try
            {
                foreach (var upd in updates.UpdateList)
                {
                    switch (upd)
                    {
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
        //  МАППЕРЫ
        // ============================================================
        private static Models.User MapUser(TL.User user) =>
        new()
        {
            Id = user.id,
            FirstName = user.first_name,
            LastName = user.last_name,
            Username = user.username,
            PhoneNumber = user.phone
        };

        private static Models.Chat? MapChat(TL.ChatBase chatBase)
        {
            if (chatBase is TL.Chat chat)
            {
                return new Models.Chat
                {
                    Id = chat.id,
                    Title = chat.title,
                    Type = ChatType.Group
                };
            }

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
        //  DISPOSE
        // ============================================================
        public void Dispose()
        {
            if (_disposed) return;

            if (_client != null)
            {
                _client.OnUpdates -= HandleUpdate;
                _client.Dispose();
            }

            _disposed = true;
        }
    }
}