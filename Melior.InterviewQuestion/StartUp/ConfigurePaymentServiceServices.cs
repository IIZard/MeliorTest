using Melior.InterviewQuestion.Data;
using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Services;
using Melior.InterviewQuestion.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Melior.InterviewQuestion.StartUp
{
    public static class ConfigurePaymentServiceServices
    {
        public const string BackupDataStoreName = "Backup";
        public const string PaymentServiceOptionsConfiguration = nameof(PaymentServiceOptionsConfiguration);

        public static void AddPaymentServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PaymentServiceOptions>(configuration.GetRequiredSection(nameof(PaymentServiceOptions)));
            services.ConfigureOptions<PaymentServiceOptions>();
            services.AddOptions<PaymentServiceOptions>();
            services.AddTransient<AccountDataStore>();
            services.AddTransient<BackupAccountDataStore>();
            services.AddScoped<IPaymentService, PaymentService>(f =>
            {
                var serviceConfig = f.GetRequiredService<IOptions<PaymentServiceOptions>>();
                IAccountDataStore requiredDataStore = string.Equals(serviceConfig.Value.DataStoreType, BackupDataStoreName, System.StringComparison.InvariantCultureIgnoreCase) ?
                    f.GetRequiredService<BackupAccountDataStore>() : f.GetRequiredService<AccountDataStore>();
                return new PaymentService(requiredDataStore);
            });
        }
    }
}
