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

            if (account is null || !IsValid(request, account))
                return FailureResult;

            account.Balance -= request.Amount; // race condition

            _accountDataStore.UpdateAccount(account);

            return SuccessResult;
        }

        private static bool IsValid(MakePaymentRequest request, Account account)
        {
            switch (request.PaymentScheme)
            {
                case PaymentScheme.Bacs:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs))
                    {
                        return false;
                    }
                    break;

                case PaymentScheme.FasterPayments:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments))
                    {
                        return false;
                    }
                    else if (account.Balance < request.Amount)
                    {
                        return false;
                    }
                    break;

                case PaymentScheme.Chaps:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps))
                    {
                        return false;
                    }
                    else if (account.Status != AccountStatus.Live)
                    {
                        return false;
                    }
                    break;
            }

            return true;
        }
    }
}
