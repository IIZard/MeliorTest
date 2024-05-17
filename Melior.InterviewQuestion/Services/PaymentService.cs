using FluentValidation;
using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Types;

namespace Melior.InterviewQuestion.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _accountDataStore;
        private readonly AbstractValidator<PaymentServiceValidatorContext> _validator;

        private static readonly MakePaymentResult FailureResult = new MakePaymentResult { Success = false };
        private static readonly MakePaymentResult SuccessResult = new MakePaymentResult { Success = true };

        public PaymentService(IAccountDataStore accountDataStore, AbstractValidator<PaymentServiceValidatorContext> validator)
        {
            _accountDataStore = accountDataStore ?? throw new System.ArgumentNullException(nameof(accountDataStore));
            _validator = validator ?? throw new System.ArgumentNullException(nameof(validator));
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var account = _accountDataStore.GetAccount(request.DebtorAccountNumber);

            if (!_validator.Validate(new PaymentServiceValidatorContext(account, request)).IsValid)
                return FailureResult;

            account.Balance -= request.Amount; // race condition

            _accountDataStore.UpdateAccount(account);

            return SuccessResult;
        }
    }
}
