using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Services;
using Melior.InterviewQuestion.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Melior.InterviewQuestion.StartUp
{
    public static class ConfigurePaymentServiceServices
    {
        public const string PaymentServiceOptionsConfiguration = nameof(PaymentServiceOptionsConfiguration);

        public static void AddPaymentServices(this IServiceCollection services)
        {
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddOptionsWithValidateOnStart<PaymentServiceOptions>(PaymentServiceOptionsConfiguration);
        }
    }
}
