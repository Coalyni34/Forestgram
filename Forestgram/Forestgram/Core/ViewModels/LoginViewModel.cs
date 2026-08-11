using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Forestgram.Core.Commands;
using Forestgram.Core.Services;

namespace Forestgram.Core.ViewModels
{
    /// <summary>
    /// ViewModel для входа в Telegram.
    /// Поддерживает любые телефонные номера (РФ, Казахстан, другие страны).
    /// </summary>
    public class LoginViewModel : IDisposable
    {
        private readonly ITelegramClient _client;
        private bool _disposed;

        // TaskCompletionSource для ожидания ввода от UI
        private TaskCompletionSource<string>? _codeTcs;

        public LoginViewModel(ITelegramClient client)
        {
            _client = client;
            LoginCommand = new AsyncRelayCommand(LoginAsync);
        }

        // ============================================================
        //  СВОЙСТВА
        // ============================================================

        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = NormalizePhoneNumber(value);
                // Обновляем статус после изменения номера
                if (!string.IsNullOrEmpty(_phoneNumber))
                {
                    Status = $"Введите код для {_phoneNumber}";
                }
            }
        }

        private string _status = "Введите номер телефона в международном формате (например, +79123456789)";
        public string Status
        {
            get => _status;
            set => _status = value;
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => _isLoading = value;
        }

        private bool _isCodeRequested;
        public bool IsCodeRequested
        {
            get => _isCodeRequested;
            set => _isCodeRequested = value;
        }

        private string _code = "";
        public string Code
        {
            get => _code;
            set => _code = value;
        }

        private bool _isPasswordRequested;
        public bool IsPasswordRequested
        {
            get => _isPasswordRequested;
            set => _isPasswordRequested = value;
        }

        // ============================================================
        //  КОМАНДЫ
        // ============================================================

        public ICommand LoginCommand { get; }

        // ============================================================
        //  СОБЫТИЯ ДЛЯ UI
        // ============================================================

        public event EventHandler<string>? CodeRequested;    // Запрос кода
        public event EventHandler<string>? PasswordRequested; // Запрос пароля (2FA)
        public event EventHandler? LoginSucceeded;

        // ============================================================
        //  ЛОГИКА
        // ============================================================

        private async Task LoginAsync()
        {
            if (IsLoading) return;

            // Валидация номера
            if (string.IsNullOrWhiteSpace(PhoneNumber) || PhoneNumber.Length < 5)
            {
                Status = "❌ Введите номер в международном формате (например, +79123456789)";
                return;
            }

            IsLoading = true;
            Status = "⏳ Отправка кода...";
            IsCodeRequested = false;
            IsPasswordRequested = false;

            try
            {
                var success = await _client.LoginAsync(PhoneNumber, (request) =>
                {
                    // Создаём TaskCompletionSource для ожидания ввода
                    _codeTcs = new TaskCompletionSource<string>();

                    // Определяем, что запрашивается: код или пароль
                    if (request?.Contains("password", StringComparison.OrdinalIgnoreCase) == true ||
                        request?.Contains("пароль", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        IsPasswordRequested = true;
                        PasswordRequested?.Invoke(this, request);
                    }
                    else
                    {
                        IsCodeRequested = true;
                        CodeRequested?.Invoke(this, request);
                    }

                    // Ждём, пока UI вызовет SubmitCode() или SubmitPassword()
                    // TaskCompletionSource<string>.Task — это Task<string>,
                    // но мы ждём результат синхронно через .GetAwaiter().GetResult()
                    // Это блокирующий вызов, но внутри делегата мы не можем использовать async/await
                    return _codeTcs.Task.GetAwaiter().GetResult();
                });

                if (success)
                {
                    Status = "✅ Вход выполнен!";
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    Status = "❌ Ошибка входа. Проверьте данные.";
                }
            }
            catch (Exception ex)
            {
                Status = $"❌ Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                IsCodeRequested = false;
                IsPasswordRequested = false;
            }
        }

        // ============================================================
        //  МЕТОДЫ ДЛЯ UI
        // ============================================================

        /// <summary>
        /// Вызывается UI, когда пользователь ввёл код подтверждения
        /// </summary>
        public void SubmitCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                _codeTcs?.SetResult("");
            }
            else
            {
                _codeTcs?.SetResult(code.Trim());
            }
            IsCodeRequested = false;
            IsPasswordRequested = false;
        }

        /// <summary>
        /// Вызывается UI, когда пользователь ввёл пароль 2FA
        /// </summary>
        public void SubmitPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _codeTcs?.SetResult("");
            }
            else
            {
                _codeTcs?.SetResult(password.Trim());
            }
            IsCodeRequested = false;
            IsPasswordRequested = false;
        }

        /// <summary>
        /// Отмена ввода
        /// </summary>
        public void CancelInput()
        {
            _codeTcs?.SetResult("");
            IsCodeRequested = false;
            IsPasswordRequested = false;
        }

        // ============================================================
        //  ХЕЛПЕРЫ
        // ============================================================

        /// <summary>
        /// Нормализует номер телефона: убирает лишние пробелы, оставляет только цифры и +
        /// </summary>
        private static string NormalizePhoneNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            var trimmed = input.Trim();

            // Если номер начинается с 8 (РФ) — заменяем на +7
            if (trimmed.StartsWith("8") && trimmed.Length >= 11)
            {
                trimmed = "+7" + trimmed.Substring(1);
            }

            // Если номер не начинается с + — добавляем +
            if (!trimmed.StartsWith("+"))
            {
                trimmed = "+" + trimmed;
            }

            return trimmed;
        }

        // ============================================================
        //  IDisposable
        // ============================================================

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}