using Melior.InterviewQuestion.Data;
using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Services;
using Melior.InterviewQuestion.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Melior.InterviewQuestion.StartUp
{
    public static class ConfigurePaymentServiceServices
    {
        public const string PaymentServiceOptionsConfiguration = nameof(PaymentServiceOptionsConfiguration);

        public static void AddPaymentServices(this IServiceCollection services)
        {
            services.AddOptionsWithValidateOnStart<PaymentServiceOptions>(PaymentServiceOptionsConfiguration);
            services.AddTransient<IAccountDataStore, AccountDataStore>();
            services.AddTransient<IAccountDataStore, BackupAccountDataStore>();
            services.AddScoped<IPaymentService, PaymentService>(f => 
            new PaymentService(f.GetRequiredService<IOptionsSnapshot<PaymentServiceOptions>>(),
                f.GetRequiredService<AccountDataStore>(),
                f.GetRequiredService<BackupAccountDataStore>()
            ));
        }
    }
}
