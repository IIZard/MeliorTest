using Microsoft.Extensions.Options;

namespace Melior.InterviewQuestion.Types
{
    public record PaymentServiceOptions() : IConfigureOptions<PaymentServiceOptions>
    {
        public string DataStoreType { get; set; }

        public void Configure(PaymentServiceOptions options)
        {
            DataStoreType = options.DataStoreType;
        }
    }
}
