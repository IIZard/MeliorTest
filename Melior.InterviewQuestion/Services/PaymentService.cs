using Melior.InterviewQuestion.Data;
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

            var result = new MakePaymentResult();

            switch (request.PaymentScheme)
            {
                case PaymentScheme.Bacs:
                    if (account == null)
                    {
                        result.Success = false;
                    }
                    else if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs))
                    {
                        result.Success = false;
                    }
                    break;

                case PaymentScheme.FasterPayments:
                    if (account == null)
                    {
                        result.Success = false;
                    }
                    else if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments))
                    {
                        result.Success = false;
                    }
                    else if (account.Balance < request.Amount)
                    {
                        result.Success = false;
                    }
                    break;

                case PaymentScheme.Chaps:
                    if (account == null)
                    {
                        result.Success = false;
                    }
                    else if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps))
                    {
                        result.Success = false;
                    }
                    else if (account.Status != AccountStatus.Live)
                    {
                        result.Success = false;
                    }
                    break;
            }

            if (result.Success)
            {
                account.Balance -= request.Amount;

                if (RequestedDataStoreTypeIsBackup(dataStoreType))
                {
                    _backupAccountDataStore.UpdateAccount(account);
                }
                else
                {
                    _liveAccountDataStore.UpdateAccount(account);
                }
            }

            return result;
        }

        private static bool RequestedDataStoreTypeIsBackup(string dataStoreType) => string.Equals(dataStoreType, BackupDataStoreName, System.StringComparison.InvariantCultureIgnoreCase);
    }
}
