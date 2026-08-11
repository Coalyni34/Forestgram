// ============================================================
// [EN] MAIN VIEW MODEL - Core UI logic: chats, messages, sending
// [RU] ГЛАВНАЯ МОДЕЛЬ - Основная логика UI: чаты, сообщения, отправка
// [ZH] 主视图模型 - 核心UI逻辑：聊天、消息、发送
// [FA] VIEW MODEL اصلی - منطق اصلی UI: چت‌ها، پیام‌ها، ارسال
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Forestgram.Core.Commands;
using Forestgram.Core.Services;

namespace Forestgram.Core.ViewModels
{
    // ============================================================
    // [EN] Main ViewModel for the chat application
    // [RU] Главный ViewModel для чат-приложения
    // [ZH] 聊天应用程序的主视图模型
    // [FA] ViewModel اصلی برای برنامه چت
    // ============================================================
    public class MainViewModel : IDisposable, INotifyPropertyChanged
    {
        // ============================================================
        // [EN] Private fields
        // [RU] Приватные поля
        // [ZH] 私有字段
        // [FA] فیلدهای خصوصی
        // ============================================================
        private readonly IDialogService _dialogService; // [EN] Dialog service / [RU] Сервис диалогов / [ZH] 对话服务 / [FA] سرویس گفتگوها
        private readonly IMessageService _messageService; // [EN] Message service / [RU] Сервис сообщений / [ZH] 消息服务 / [FA] سرویس پیام
        private bool _disposed; // [EN] Disposal flag / [RU] Флаг удаления / [ZH] 释放标记 / [FA] پرچم آزادسازی

        // ============================================================
        // [EN] Observable collections for UI binding
        // [RU] Наблюдаемые коллекции для привязки UI
        // [ZH] 用于UI绑定的可观察集合
        // [FA] مجموعه‌های قابل مشاهده برای اتصال UI
        // ============================================================
        public ObservableCollection<Models.Chat> Dialogs { get; } = new(); // [EN] Chat list / [RU] Список чатов / [ZH] 聊天列表 / [FA] لیست چت‌ها
        public ObservableCollection<Models.Message> Messages { get; } = new(); // [EN] Message list / [RU] Список сообщений / [ZH] 消息列表 / [FA] لیست پیام‌ها

        // ============================================================
        // [EN] PROPERTIES - Bound to UI
        // [RU] СВОЙСТВА - Привязаны к UI
        // [ZH] 属性 - 绑定到UI
        // [FA] خصوصیات - متصل به UI
        // ============================================================

        // [EN] Currently selected chat
        // [RU] Выбранный чат
        // [ZH] 当前选中的聊天
        // [FA] چت انتخاب شده فعلی
        private Models.Chat? _selectedChat;
        public Models.Chat? SelectedChat
        {
            get => _selectedChat;
            set
            {
                if (_selectedChat?.Id == value?.Id) return; // [EN] Prevent reloading same chat / [RU] Предотвращаем перезагрузку того же чата / [ZH] 防止重新加载同一聊天 / [FA] جلوگیری از بارگذاری مجدد چت مشابه
                _selectedChat = value;
                OnPropertyChanged();
                OnSelectedChatChanged(); // [EN] Trigger messages load / [RU] Запускает загрузку сообщений / [ZH] 触发消息加载 / [FA] فعال‌سازی بارگذاری پیام‌ها
            }
        }

        // [EN] Loading indicator
        // [RU] Индикатор загрузки
        // [ZH] 加载指示器
        // [FA] نشانگر بارگذاری
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        // [EN] Message text input
        // [RU] Поле ввода сообщения
        // [ZH] 消息文本输入
        // [FA] ورودی متن پیام
        private string _messageText = string.Empty;
        public string MessageText
        {
            get => _messageText;
            set
            {
                if (_messageText == value) return;
                _messageText = value;
                OnPropertyChanged();
            }
        }

        // ============================================================
        // [EN] COMMANDS - Bound to UI buttons
        // [RU] КОМАНДЫ - Привязаны к кнопкам UI
        // [ZH] 命令 - 绑定到UI按钮
        // [FA] دستورات - متصل به دکمه‌های UI
        // ============================================================
        public ICommand LoadDialogsCommand { get; } // [EN] Load chats command / [RU] Команда загрузки чатов / [ZH] 加载聊天命令 / [FA] دستور بارگذاری چت‌ها
        public ICommand SendMessageCommand { get; } // [EN] Send message command / [RU] Команда отправки сообщения / [ZH] 发送消息命令 / [FA] دستور ارسال پیام

        // ============================================================
        // [EN] Constructor - Initialize services and commands
        // [RU] Конструктор - Инициализация сервисов и команд
        // [ZH] 构造函数 - 初始化服务和命令
        // [FA] سازنده - راه‌اندازی سرویس‌ها و دستورات
        // ============================================================
        public MainViewModel(IDialogService dialogService, IMessageService messageService)
        {
            _dialogService = dialogService;
            _messageService = messageService;

            LoadDialogsCommand = new AsyncRelayCommand(LoadDialogsAsync);
            SendMessageCommand = new AsyncRelayCommand(SendCurrentMessageAsync);

            // [EN] Subscribe to new messages
            // [RU] Подписываемся на новые сообщения
            // [ZH] 订阅新消息
            // [FA] اشتراک در پیام‌های جدید
            _messageService.SubscribeToNewMessages(OnNewMessage);

            // [EN] Auto-load chats on startup
            // [RU] Автоматическая загрузка чатов при запуске
            // [ZH] 启动时自动加载聊天
            // [FA] بارگذاری خودکار چت‌ها در هنگام راه‌اندازی
            _ = LoadDialogsAsync();
        }

        // ============================================================
        // [EN] Load dialogs from Telegram
        // [RU] Загрузить диалоги из Telegram
        // [ZH] 从Telegram加载对话
        // [FA] بارگذاری گفتگوها از تلگرام
        // ============================================================
        private async Task LoadDialogsAsync()
        {
            if (IsLoading) return;

            IsLoading = true;

            try
            {
                var dialogs = await _dialogService.GetDialogsAsync(true);
                Dialogs.Clear();
                foreach (var dialog in dialogs)
                    Dialogs.Add(dialog);
            }
            catch (Exception ex)
            {
                // [EN] TODO: Show error to user via event
                // [RU] TODO: Показать ошибку пользователю через событие
                // [ZH] TODO: 通过事件向用户显示错误
                // [FA] TODO: نمایش خطا به کاربر از طریق رویداد
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] LoadDialogs error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ============================================================
        // [EN] Called when selected chat changes
        // [RU] Вызывается при изменении выбранного чата
        // [ZH] 当选中的聊天更改时调用
        // [FA] زمانی که چت انتخاب شده تغییر می‌کند، فراخوانی می‌شود
        // ============================================================
        private void OnSelectedChatChanged()
        {
            _ = LoadMessagesAsync(); // [EN] Load messages for selected chat / [RU] Загрузить сообщения для выбранного чата / [ZH] 为选中的聊天加载消息 / [FA] بارگذاری پیام‌ها برای چت انتخاب شده
        }

        // ============================================================
        // [EN] Load messages for selected chat
        // [RU] Загрузить сообщения для выбранного чата
        // [ZH] 为选中的聊天加载消息
        // [FA] بارگذاری پیام‌ها برای چت انتخاب شده
        // ============================================================
        private async Task LoadMessagesAsync()
        {
            if (SelectedChat == null) return;

            IsLoading = true;

            try
            {
                var messages = await _messageService.GetMessagesAsync(SelectedChat.Id);
                Messages.Clear();
                foreach (var msg in messages)
                    Messages.Add(msg);
            }
            catch (Exception ex)
            {
                // [EN] TODO: Show error to user via event
                // [RU] TODO: Показать ошибку пользователю через событие
                // [ZH] TODO: 通过事件向用户显示错误
                // [FA] TODO: نمایش خطا به کاربر از طریق رویداد
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] LoadMessages error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ============================================================
        // [EN] Send current message
        // [RU] Отправить текущее сообщение
        // [ZH] 发送当前消息
        // [FA] ارسال پیام فعلی
        // ============================================================
        private async Task SendCurrentMessageAsync()
        {
            if (SelectedChat == null || string.IsNullOrWhiteSpace(MessageText))
                return;

            var text = MessageText.Trim();

            try
            {
                var sent = await _messageService.SendMessageAsync(SelectedChat.Id, text);
                if (sent)
                {
                    // [EN] Message will arrive via OnNewMessage event
                    // [RU] Сообщение придёт через событие OnNewMessage
                    // [ZH] 消息将通过OnNewMessage事件到达
                    // [FA] پیام از طریق رویداد OnNewMessage دریافت می‌شود
                    MessageText = "";
                }
                else
                {
                    // [EN] TODO: Show error to user via event
                    // [RU] TODO: Показать ошибку пользователю через событие
                    // [ZH] TODO: 通过事件向用户显示错误
                    // [FA] TODO: نمایش خطا به کاربر از طریق رویداد
                    System.Diagnostics.Debug.WriteLine("[MainViewModel] SendMessage failed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] SendMessage error: {ex.Message}");
            }
        }

        // ============================================================
        // [EN] Handle new incoming messages
        // [RU] Обработка новых входящих сообщений
        // [ZH] 处理新的传入消息
        // [FA] پردازش پیام‌های ورودی جدید
        // ============================================================
        private void OnNewMessage(object? sender, Models.Message message)
        {
            // [EN] Add message to list if it belongs to current chat
            // [RU] Добавляем сообщение в список, если оно относится к текущему чату
            // [ZH] 如果消息属于当前聊天，则将其添加到列表中
            // [FA] اگر پیام متعلق به چت فعلی است، به لیست اضافه کن
            if (SelectedChat != null && message.ChatId == SelectedChat.Id)
            {
                Messages.Add(message);
            }
        }

        // ============================================================
        // [EN] INotifyPropertyChanged - Notify UI of property changes
        // [RU] INotifyPropertyChanged - Уведомление UI об изменениях свойств
        // [ZH] INotifyPropertyChanged - 通知UI属性更改
        // [FA] INotifyPropertyChanged - اطلاع‌رسانی به UI در مورد تغییرات خصوصیات
        // ============================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ============================================================
        // [EN] IDisposable - Clean up resources
        // [RU] IDisposable - Очистка ресурсов
        // [ZH] IDisposable - 清理资源
        // [FA] IDisposable - پاکسازی منابع
        // ============================================================
        public void Dispose()
        {
            if (_disposed) return;

            // [EN] Unsubscribe from new messages
            // [RU] Отписываемся от новых сообщений
            // [ZH] 取消订阅新消息
            // [FA] لغو اشتراک از پیام‌های جدید
            _messageService.UnsubscribeFromNewMessages(OnNewMessage);
            
            // [EN] Clear collections
            // [RU] Очищаем коллекции
            // [ZH] 清空集合
            // [FA] پاک کردن مجموعه‌ها
            Dialogs.Clear();
            Messages.Clear();

            _disposed = true;
        }
    }
}