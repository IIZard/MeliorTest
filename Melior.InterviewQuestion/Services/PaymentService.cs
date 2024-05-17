using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Types;

namespace Melior.InterviewQuestion.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStore _accountDataStore;

        private static readonly MakePaymentResult FailureResult = new MakePaymentResult { Success = false };
        private static readonly MakePaymentResult SuccessResult = new MakePaymentResult { Success = true };

        public PaymentService(IAccountDataStore accountDataStore)
        {
            _accountDataStore = accountDataStore ?? throw new System.ArgumentNullException(nameof(accountDataStore));
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var account = _accountDataStore.GetAccount(request.DebtorAccountNumber);

            if (account is null)
                return FailureResult;

            switch (request.PaymentScheme)
            {
                case PaymentScheme.Bacs:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs))
                    {
                        return FailureResult;
                    }
                    break;

                case PaymentScheme.FasterPayments:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments))
                    {
                        return FailureResult;
                    }
                    else if (account.Balance < request.Amount)
                    {
                        return FailureResult;
                    }
                    break;

                case PaymentScheme.Chaps:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps))
                    {
                        return FailureResult;
                    }
                    else if (account.Status != AccountStatus.Live)
                    {
                        return FailureResult;
                    }
                    break;
            }

            account.Balance -= request.Amount; // race condition

            _accountDataStore.UpdateAccount(account);

            return SuccessResult;
        }
    }
}
