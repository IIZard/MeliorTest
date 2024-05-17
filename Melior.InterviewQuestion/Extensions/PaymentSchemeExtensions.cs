using Melior.InterviewQuestion.Types;

namespace Melior.InterviewQuestion.Extensions
{
    public static class PaymentSchemeExtensions
    {
        public static AllowedPaymentSchemes ConvertToAlowedPaymentScheme(this PaymentScheme paymentScheme)
        {
            switch (paymentScheme)
            {
                case PaymentScheme.Chaps:
                    return AllowedPaymentSchemes.Chaps;
                case PaymentScheme.Bacs:
                    return AllowedPaymentSchemes.Bacs;
                case PaymentScheme.FasterPayments:
                    return AllowedPaymentSchemes.FasterPayments;
                default:
                    return 0;
            }
        }
    }
}
