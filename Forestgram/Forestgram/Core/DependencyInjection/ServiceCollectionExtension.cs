using Forestgram.Core.Services;
using Forestgram.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Forestgram.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddForestgramCore(
            this IServiceCollection services,
            string apiId,
            string apiHash)
        {
            // ============================================================
            // 1. Хранилище (IStorage)
            // ============================================================
            services.AddSingleton<IStorage, FileStorage>();

            // ============================================================
            // 2. Клиент Telegram (ITelegramClient)
            // ============================================================
            services.AddSingleton<ITelegramClient>(sp =>
            {
                var storage = sp.GetRequiredService<IStorage>();
                return new TelegramClient(storage, apiId, apiHash);
            });

            // ============================================================
            // 3. Сервис диалогов (IDialogService)
            // ============================================================
            services.AddSingleton<IDialogService, DialogService>();

            // ============================================================
            // 4. Сервис сообщений (IMessageService)
            // ============================================================
            services.AddSingleton<IMessageService, MessageService>();

            // ============================================================
            // 5. ViewModel для входа (LoginViewModel)
            // ============================================================
            services.AddTransient<LoginViewModel>();

            // ============================================================
            // 6. Главная ViewModel (MainViewModel)
            // ============================================================
            services.AddTransient<MainViewModel>();

            return services;
        }
    }
}