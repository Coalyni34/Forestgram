// [EN] DI registration: all Core services and ViewModels
// [RU] DI-регистрация: все сервисы и ViewModel
// [ZH] 依赖注入注册：所有核心服务和视图模型
// [FA] ثبت وابستگی: همه سرویس‌ها و ViewModelها

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
            // [EN] Storage / [RU] Хранилище / [ZH] 存储 / [FA] حافظه
            services.AddSingleton<IStorage, FileStorage>();

            // [EN] Telegram client / [RU] Клиент Telegram / [ZH] Telegram客户端 / [FA] کلاینت تلگرام
            services.AddSingleton<ITelegramClient>(sp =>
            {
                var storage = sp.GetRequiredService<IStorage>();
                return new TelegramClient(storage, apiId, apiHash);
            });

            // [EN] Services / [RU] Сервисы / [ZH] 服务 / [FA] سرویس‌ها
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IMessageService, MessageService>();

            // [EN] ViewModels / [RU] ViewModel / [ZH] 视图模型 / [FA] ViewModelها
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();

            return services;
        }
    }
}