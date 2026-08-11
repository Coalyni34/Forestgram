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
    public class MainViewModel : IDisposable, INotifyPropertyChanged
    {
        private readonly IDialogService _dialogService;
        private readonly IMessageService _messageService;
        private bool _disposed;

        public ObservableCollection<Models.Chat> Dialogs { get; } = new();
        public ObservableCollection<Models.Message> Messages { get; } = new();

        private Models.Chat? _selectedChat;
        public Models.Chat? SelectedChat
        {
            get => _selectedChat;
            set
            {
                if (_selectedChat?.Id == value?.Id) return; 
                _selectedChat = value;
                OnPropertyChanged();
                OnSelectedChatChanged();
            }
        }

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

        // Команды 
        public ICommand LoadDialogsCommand { get; }
        public ICommand SendMessageCommand { get; }

        public MainViewModel(IDialogService dialogService, IMessageService messageService)
        {
            _dialogService = dialogService;
            _messageService = messageService;

            LoadDialogsCommand = new AsyncRelayCommand(LoadDialogsAsync);
            SendMessageCommand = new AsyncRelayCommand(SendCurrentMessageAsync);

            // Подписываемся на новые сообщения
            _messageService.SubscribeToNewMessages(OnNewMessage);

            // Автоматически загружаем чаты при создании
            _ = LoadDialogsAsync();
        }

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
                // TODO: Сообщить пользователю об ошибке через событие
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] LoadDialogs error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSelectedChatChanged()
        {
            _ = LoadMessagesAsync();
        }

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
                // TODO: Сообщить пользователю об ошибке через событие
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] LoadMessages error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

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
                    // Сообщение отправится, и оно придёт через OnNewMessage
                    MessageText = "";
                }
                else
                {
                    // TODO: Сообщить об ошибке отправки
                    System.Diagnostics.Debug.WriteLine("[MainViewModel] SendMessage failed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] SendMessage error: {ex.Message}");
            }
        }

        private void OnNewMessage(object? sender, Models.Message message)
        {
            // Если сообщение из текущего чата — добавляем в список
            if (SelectedChat != null && message.ChatId == SelectedChat.Id)
            {
                Messages.Add(message);
            }
        }

        // ============================================================
        //  INotifyPropertyChanged
        // ============================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ============================================================
        //  IDisposable
        // ============================================================
        public void Dispose()
        {
            if (_disposed) return;

            _messageService.UnsubscribeFromNewMessages(OnNewMessage);
            Dialogs.Clear();
            Messages.Clear();

            _disposed = true;
        }
    }
}