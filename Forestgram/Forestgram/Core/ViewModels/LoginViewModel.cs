// ============================================================
// [EN] LOGIN VIEW MODEL - Handles Telegram login with phone, code, 2FA
// [RU] МОДЕЛЬ ВХОДА - Обрабатывает вход в Telegram с телефоном, кодом, 2FA
// [ZH] 登录视图模型 - 处理Telegram登录（电话、验证码、两步验证）
// [FA] VIEW MODEL ورود - مدیریت ورود به تلگرام با تلفن، کد، تایید دو مرحله‌ای
// ============================================================

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Forestgram.Core.Commands;
using Forestgram.Core.Services;

namespace Forestgram.Core.ViewModels
{
    // ============================================================
    // [EN] ViewModel for login screen
    // [RU] ViewModel для экрана входа
    // [ZH] 登录界面的视图模型
    // [FA] ViewModel صفحه ورود
    // ============================================================
    /// <summary>
    /// [EN] ViewModel for Telegram login. Supports any phone numbers worldwide.
    /// [RU] ViewModel для входа в Telegram. Поддерживает любые номера мира.
    /// [ZH] Telegram登录的视图模型。支持全球任何电话号码。
    /// [FA] ViewModel ورود به تلگرام. پشتیبانی از هر شماره تلفن در جهان.
    /// </summary>
    public class LoginViewModel : IDisposable
    {
        // [EN] Telegram client for authentication
        // [RU] Клиент Telegram для аутентификации
        // [ZH] 用于身份验证的Telegram客户端
        // [FA] کلاینت تلگرام برای احراز هویت
        private readonly ITelegramClient _client;
        
        // [EN] Disposal flag
        // [RU] Флаг удаления
        // [ZH] 释放标记
        // [FA] پرچم آزادسازی
        private bool _disposed;

        // [EN] TaskCompletionSource for waiting on UI input (code/password)
        // [RU] TaskCompletionSource для ожидания ввода от UI (код/пароль)
        // [ZH] 用于等待UI输入（验证码/密码）的TaskCompletionSource
        // [FA] TaskCompletionSource برای انتظار ورودی از UI (کد/رمز عبور)
        private TaskCompletionSource<string>? _codeTcs;

        // ============================================================
        // [EN] Constructor
        // [RU] Конструктор
        // [ZH] 构造函数
        // [FA] سازنده
        // ============================================================
        public LoginViewModel(ITelegramClient client)
        {
            _client = client;
            LoginCommand = new AsyncRelayCommand(LoginAsync);
        }

        // ============================================================
        // [EN] PROPERTIES - Bound to UI
        // [RU] СВОЙСТВА - Привязаны к UI
        // [ZH] 属性 - 绑定到UI
        // [FA] خصوصیات - متصل به UI
        // ============================================================

        // [EN] Phone number input (auto-normalized)
        // [RU] Ввод номера телефона (авто-нормализация)
        // [ZH] 电话号码输入（自动规范化）
        // [FA] ورودی شماره تلفن (به‌طور خودکار نرمال‌سازی می‌شود)
        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = NormalizePhoneNumber(value);
                if (!string.IsNullOrEmpty(_phoneNumber))
                {
                    Status = $"Введите код для {_phoneNumber}";
                }
            }
        }

        // [EN] Status message for UI
        // [RU] Сообщение статуса для UI
        // [ZH] UI状态消息
        // [FA] پیام وضعیت برای UI
        private string _status = "Введите номер телефона в международном формате (например, +79123456789)";
        public string Status
        {
            get => _status;
            set => _status = value;
        }

        // [EN] Loading indicator
        // [RU] Индикатор загрузки
        // [ZH] 加载指示器
        // [FA] نشانگر بارگذاری
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => _isLoading = value;
        }

        // [EN] Flag indicating code is being requested
        // [RU] Флаг, указывающий на запрос кода
        // [ZH] 表示正在请求验证码的标志
        // [FA] پرچم نشان‌دهنده درخواست کد
        private bool _isCodeRequested;
        public bool IsCodeRequested
        {
            get => _isCodeRequested;
            set => _isCodeRequested = value;
        }

        // [EN] Code input field
        // [RU] Поле ввода кода
        // [ZH] 验证码输入字段
        // [FA] فیلد ورودی کد
        private string _code = "";
        public string Code
        {
            get => _code;
            set => _code = value;
        }

        // [EN] Flag indicating 2FA password is being requested
        // [RU] Флаг, указывающий на запрос пароля 2FA
        // [ZH] 表示正在请求两步验证密码的标志
        // [FA] پرچم نشان‌دهنده درخواست رمز تایید دو مرحله‌ای
        private bool _isPasswordRequested;
        public bool IsPasswordRequested
        {
            get => _isPasswordRequested;
            set => _isPasswordRequested = value;
        }

        // ============================================================
        // [EN] COMMANDS
        // [RU] КОМАНДЫ
        // [ZH] 命令
        // [FA] دستورات
        // ============================================================

        // [EN] Login command (triggered by UI button)
        // [RU] Команда входа (вызывается кнопкой UI)
        // [ZH] 登录命令（由UI按钮触发）
        // [FA] دستور ورود (توسط دکمه UI فعال می‌شود)
        public ICommand LoginCommand { get; }

        // ============================================================
        // [EN] UI EVENTS
        // [RU] СОБЫТИЯ ДЛЯ UI
        // [ZH] UI事件
        // [FA] رویدادهای UI
        // ============================================================

        // [EN] Raised when verification code is needed
        // [RU] Срабатывает, когда требуется код подтверждения
        // [ZH] 当需要验证码时触发
        // [FA] زمانی که کد تایید نیاز است، فعال می‌شود
        public event EventHandler<string>? CodeRequested;

        // [EN] Raised when 2FA password is needed
        // [RU] Срабатывает, когда требуется пароль 2FA
        // [ZH] 当需要两步验证密码时触发
        // [FA] زمانی که رمز تایید دو مرحله‌ای نیاز است، فعال می‌شود
        public event EventHandler<string>? PasswordRequested;

        // [EN] Raised when login is successful
        // [RU] Срабатывает при успешном входе
        // [ZH] 登录成功时触发
        // [FA] زمانی که ورود موفق باشد، فعال می‌شود
        public event EventHandler? LoginSucceeded;

        // ============================================================
        // [EN] LOGIC - Core login flow
        // [RU] ЛОГИКА - Основной поток входа
        // [ZH] 逻辑 - 核心登录流程
        // [FA] منطق - جریان اصلی ورود
        // ============================================================

        // [EN] Main login method (executed by LoginCommand)
        // [RU] Основной метод входа (выполняется LoginCommand)
        // [ZH] 主要登录方法（由LoginCommand执行）
        // [FA] متد اصلی ورود (توسط LoginCommand اجرا می‌شود)
        private async Task LoginAsync()
        {
            // [EN] Prevent multiple concurrent logins
            // [RU] Защита от параллельных входов
            // [ZH] 防止并发登录
            // [FA] جلوگیری از ورود همزمان
            if (IsLoading) return;

            // [EN] Validate phone number
            // [RU] Проверка номера телефона
            // [ZH] 验证电话号码
            // [FA] اعتبارسنجی شماره تلفن
            if (string.IsNullOrWhiteSpace(PhoneNumber) || PhoneNumber.Length < 5)
            {
                Status = "❌ Введите номер в международном формате (например, +79123456789)";
                return;
            }

            // [EN] Set loading state
            // [RU] Установка состояния загрузки
            // [ZH] 设置加载状态
            // [FA] تنظیم وضعیت بارگذاری
            IsLoading = true;
            Status = "⏳ Отправка кода...";
            IsCodeRequested = false;
            IsPasswordRequested = false;

            try
            {
                // [EN] Call Telegram client login with code provider callback
                // [RU] Вызов входа в клиент Telegram с обратным вызовом для кода
                // [ZH] 使用验证码提供者回调调用Telegram客户端登录
                // [FA] فراخوانی ورود کلاینت تلگرام با بازگشت کد
                var success = await _client.LoginAsync(PhoneNumber, (request) =>
                {
                    // [EN] Create TaskCompletionSource to wait for UI input
                    // [RU] Создаём TaskCompletionSource для ожидания ввода UI
                    // [ZH] 创建TaskCompletionSource以等待UI输入
                    // [FA] ایجاد TaskCompletionSource برای انتظار ورودی UI
                    _codeTcs = new TaskCompletionSource<string>();

                    // [EN] Determine if password or code is requested
                    // [RU] Определяем, запрашивается пароль или код
                    // [ZH] 确定请求的是密码还是验证码
                    // [FA] تشخیص اینکه رمز عبور یا کد درخواست شده است
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

                    // [EN] Synchronously wait for UI to call SubmitCode/SubmitPassword
                    // [RU] Синхронное ожидание вызова SubmitCode/SubmitPassword из UI
                    // [ZH] 同步等待UI调用SubmitCode/SubmitPassword
                    // [FA] انتظار همزمان برای فراخوانی SubmitCode/SubmitPassword از UI
                    // [WARNING] GetAwaiter().GetResult() blocks the thread
                    // [ПРЕДУПРЕЖДЕНИЕ] GetAwaiter().GetResult() блокирует поток
                    // [警告] GetAwaiter().GetResult() 阻塞线程
                    // [اخطار] GetAwaiter().GetResult() ترد را مسدود می‌کند
                    return _codeTcs.Task.GetAwaiter().GetResult();
                });

                // [EN] Process login result
                // [RU] Обработка результата входа
                // [ZH] 处理登录结果
                // [FA] پردازش نتیجه ورود
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
                // [EN] Reset loading state
                // [RU] Сброс состояния загрузки
                // [ZH] 重置加载状态
                // [FA] بازنشانی وضعیت بارگذاری
                IsLoading = false;
                IsCodeRequested = false;
                IsPasswordRequested = false;
            }
        }

        // ============================================================
        // [EN] UI METHODS - Called from UI after user input
        // [RU] МЕТОДЫ ДЛЯ UI - Вызываются из UI после ввода пользователя
        // [ZH] UI方法 - 在用户输入后从UI调用
        // [FA] متدهای UI - پس از ورود کاربر از UI فراخوانی می‌شوند
        // ============================================================

        /// <summary>
        /// [EN] Called by UI when user enters verification code
        /// [RU] Вызывается UI, когда пользователь ввёл код подтверждения
        /// [ZH] 当用户输入验证码时由UI调用
        /// [FA] زمانی که کاربر کد تایید را وارد کرد، توسط UI فراخوانی می‌شود
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
        /// [EN] Called by UI when user enters 2FA password
        /// [RU] Вызывается UI, когда пользователь ввёл пароль 2FA
        /// [ZH] 当用户输入两步验证密码时由UI调用
        /// [FA] زمانی که کاربر رمز تایید دو مرحله‌ای را وارد کرد، توسط UI فراخوانی می‌شود
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
        /// [EN] Called by UI when user cancels input
        /// [RU] Вызывается UI при отмене ввода
        /// [ZH] 当用户取消输入时由UI调用
        /// [FA] زمانی که کاربر ورودی را لغو کرد، توسط UI فراخوانی می‌شود
        /// </summary>
        public void CancelInput()
        {
            _codeTcs?.SetResult("");
            IsCodeRequested = false;
            IsPasswordRequested = false;
        }

        // ============================================================
        // [EN] HELPERS
        // [RU] ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // [ZH] 辅助方法
        // [FA] متدهای کمکی
        // ============================================================

        /// <summary>
        /// [EN] Normalizes phone number: removes spaces, converts 8 to +7
        /// [RU] Нормализует номер телефона: убирает пробелы, преобразует 8 в +7
        /// [ZH] 规范化电话号码：删除空格，将8转换为+7
        /// [FA] نرمال‌سازی شماره تلفن: حذف فاصله‌ها، تبدیل 8 به +7
        /// </summary>
        private static string NormalizePhoneNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            var trimmed = input.Trim();

            // [EN] Convert Russian 8 to +7
            // [RU] Преобразование российского 8 в +7
            // [ZH] 将俄罗斯的8转换为+7
            // [FA] تبدیل 8 روسی به +7
            if (trimmed.StartsWith("8") && trimmed.Length >= 11)
            {
                trimmed = "+7" + trimmed.Substring(1);
            }

            // [EN] Add + if missing
            // [RU] Добавляем +, если отсутствует
            // [ZH] 如果缺少+则添加
            // [FA] اگر + وجود نداشت، اضافه کن
            if (!trimmed.StartsWith("+"))
            {
                trimmed = "+" + trimmed;
            }

            return trimmed;
        }

        // ============================================================
        // [EN] IDISPOSABLE - Clean up resources
        // [RU] IDISPOSABLE - Очистка ресурсов
        // [ZH] 释放 - 清理资源
        // [FA] آزادسازی - پاکسازی منابع
        // ============================================================

        public void Dispose()
        {
            if (_disposed) return;
            // [EN] No specific resources to dispose
            // [RU] Нет специфических ресурсов для очистки
            // [ZH] 没有需要释放的特定资源
            // [FA] منبع خاصی برای آزادسازی وجود ندارد
            _disposed = true;
        }
    }
}