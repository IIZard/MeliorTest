using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Services;
using Melior.InterviewQuestion.Types;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace Melior.InterviewQuestion.UnitTests.Tests.Services
{
    [TestFixture]
    public class PaymentServiceTests
    {
        private IPaymentService _sut;
        private AutoMocker _container;

        private const string BackupDataStoreName = "Backup";
        private const string LiveDataStoreName = "Live";

        [SetUp]
        public void SetUp()
        {
            _container = new AutoMocker(); 
            _sut = _container.CreateInstance<PaymentService>();
        }

        [TestCaseSource(nameof(AllPaymentServiceTestData))]
        public void MakePayment_WithNullAccount_IsNotSuccess(string dataStoreType, PaymentScheme paymentScheme)
        {
            _container.GetMock<IOptionsSnapshot<PaymentServiceOptions>>()
                .SetupGet(m => m.Value).Returns(new PaymentServiceOptions(dataStoreType));
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme };

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.False);
        }

        [TestCaseSource(nameof(AllPaymentServiceTestData))]
        public void MakePayment_WithNoAllowedPaymentScheme_IsNotSuccess(string dataStoreType, PaymentScheme paymentScheme)
        {
            var testaccount = new Account { AllowedPaymentSchemes = 0 };
            _container.GetMock<IOptionsSnapshot<PaymentServiceOptions>>()
                .SetupGet(m => m.Value).Returns(new PaymentServiceOptions(dataStoreType));
            _container.GetMock<IAccountDataStore>().Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme };

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.False);
        }

        [TestCaseSource(nameof(FasterPaymentsPaymentServiceTestData))]
        public void MakePayment_FasterPayments_WithLowBalance_IsNotSuccess(string dataStoreType, PaymentScheme paymentScheme)
        {
            var testbalance = 1000;
            var testaccount = new Account { AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments, Balance = testbalance };
            _container.GetMock<IOptionsSnapshot<PaymentServiceOptions>>()
                .SetupGet(m => m.Value).Returns(new PaymentServiceOptions(dataStoreType));
            _container.GetMock<IAccountDataStore>().Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme, Amount = testbalance * 2 };

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.False);
        }

        [TestCaseSource(nameof(ChapsPaymentServiceTestData))]
        public void MakePayment_Chaps_WithoutLiveStatus_IsNotSuccess(string dataStoreType, PaymentScheme paymentScheme)
        {
            var testaccount = new Account { AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps, Status = AccountStatus.Disabled };
            _container.GetMock<IOptionsSnapshot<PaymentServiceOptions>>()
                .SetupGet(m => m.Value).Returns(new PaymentServiceOptions(dataStoreType));
            _container.GetMock<IAccountDataStore>().Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme };

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.False);
        }

        [TestCaseSource(nameof(AllPaymentServiceTestData))]
        public void MakePayment_WithCorrectSettings_IsSuccess(string dataStoreType, PaymentScheme paymentScheme)
        {
            var testbalance = 1000;
            var testaccount = new Account { 
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps, 
                Balance = testbalance * 2,
                Status = AccountStatus.Live
            };
            _container.GetMock<IOptionsSnapshot<PaymentServiceOptions>>()
                .SetupGet(m => m.Value).Returns(new PaymentServiceOptions(dataStoreType));
            _container.GetMock<IAccountDataStore>().Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme, Amount = testbalance };

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.True);
        }

        public static IEnumerable<TestCaseData> AllPaymentServiceTestData()
        {
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.Bacs);
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.FasterPayments);
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.Chaps);
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.Bacs);
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.FasterPayments);
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.Chaps);
        }

        public static IEnumerable<TestCaseData> BacsPaymentServiceTestData()
        {
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.Bacs);
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.Bacs);
        }

        public static IEnumerable<TestCaseData> FasterPaymentsPaymentServiceTestData()
        {
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.FasterPayments);
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.FasterPayments);
        }

        public static IEnumerable<TestCaseData> ChapsPaymentServiceTestData()
        {
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.Chaps);
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.Chaps);
        }
    }
}
