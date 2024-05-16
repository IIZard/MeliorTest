using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Types;
using Microsoft.Extensions.Options;

namespace Melior.InterviewQuestion.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IOptionsSnapshot<PaymentServiceOptions> _paymentServiceOptions;
        private readonly IAccountDataStore _liveAccountDataStore;
        private readonly IAccountDataStore _backupAccountDataStore;

        private const string BackupDataStoreName = "Backup";

        public PaymentService(IOptionsSnapshot<PaymentServiceOptions> paymentServiceOptions,
            IAccountDataStore liveAccountDataStore,
            IAccountDataStore backupAccountDataStore)
        {
            _paymentServiceOptions = paymentServiceOptions ?? throw new System.ArgumentNullException(nameof(paymentServiceOptions));
            _liveAccountDataStore = liveAccountDataStore ?? throw new System.ArgumentNullException(nameof(liveAccountDataStore));
            _backupAccountDataStore = backupAccountDataStore ?? throw new System.ArgumentNullException(nameof(backupAccountDataStore));
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var dataStoreType = _paymentServiceOptions.Value.DataStoreType;

            var account = RequestedDataStoreTypeIsBackup(dataStoreType) ?
                _backupAccountDataStore.GetAccount(request.DebtorAccountNumber) : _liveAccountDataStore.GetAccount(request.DebtorAccountNumber);

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

            if (RequestedDataStoreTypeIsBackup(dataStoreType))
            {
                _backupAccountDataStore.UpdateAccount(account);
            }
            else
            {
                _liveAccountDataStore.UpdateAccount(account);
            }

            return SuccessResult;
        }

        private static bool RequestedDataStoreTypeIsBackup(string dataStoreType) => 
            string.Equals(dataStoreType, BackupDataStoreName, System.StringComparison.InvariantCultureIgnoreCase);

        private readonly MakePaymentResult FailureResult = new MakePaymentResult { Success = false };
        private readonly MakePaymentResult SuccessResult = new MakePaymentResult { Success = true };
    }
}
