using FluentValidation;
using Melior.InterviewQuestion.Extensions;
using Melior.InterviewQuestion.Types;

namespace Melior.InterviewQuestion.Services
{
    public class PaymentServiceValidator : AbstractValidator<(Account Account, MakePaymentRequest Request)>
    {
        public PaymentServiceValidator()
        {
            RuleFor(x => x.Account).NotNull();
            RuleFor(v => v.Account.AllowedPaymentSchemes)
                .Custom((aps, v) => aps.HasFlag(v.InstanceToValidate.Request.PaymentScheme.ConvertToAlowedPaymentScheme()));
            RuleFor(v => v.Account.Balance)
                .GreaterThan(v => v.Request.Amount)
                .When(v => v.Request.PaymentScheme == PaymentScheme.FasterPayments);
            RuleFor(v => v.Account.Status)
                .Equal(AccountStatus.Live)
                .When(v => v.Request.PaymentScheme == PaymentScheme.Chaps);
        }
    }
}
