using FluentValidation;
using Melior.InterviewQuestion.Extensions;
using Melior.InterviewQuestion.Types;

namespace Melior.InterviewQuestion.Validators
{
    public class PaymentServiceValidator : AbstractValidator<PaymentServiceValidatorContext>
    {
        public PaymentServiceValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(v => v.Account).NotNull();
            RuleFor(v => v.Account.AllowedPaymentSchemes)
                .Custom((aps, v) => 
                {
                    if (!aps.HasFlag(v.InstanceToValidate.Request.PaymentScheme.ConvertToAlowedPaymentScheme()))
                    {
                        v.AddFailure("Requested PaymentScheme not in AllowedPaymentSchemes");
                    }
                });
            RuleFor(v => v.Account.Balance)
                .GreaterThan(v => v.Request.Amount)
                .When(v => v.Request.PaymentScheme == PaymentScheme.FasterPayments);
            RuleFor(v => v.Account.Status)
                .Equal(AccountStatus.Live)
                .When(v => v.Request.PaymentScheme == PaymentScheme.Chaps);
        }
    }
}
