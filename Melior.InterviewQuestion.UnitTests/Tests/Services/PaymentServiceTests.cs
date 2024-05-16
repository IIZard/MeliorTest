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
            var testbalance = 1000m;
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
            var testbalance = 1000m;
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

        [TestCaseSource(nameof(AllPaymentServiceTestData))]
        public void MakePayment_WithCorrectSettings_UpdatesAccount_WithNewBalance(string dataStoreType, PaymentScheme paymentScheme)
        {
            var expectedBalance = 1000m;
            var testaccount = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
                Balance = expectedBalance * 2,
                Status = AccountStatus.Live
            };
            _container.GetMock<IOptionsSnapshot<PaymentServiceOptions>>()
                .SetupGet(m => m.Value).Returns(new PaymentServiceOptions(dataStoreType));
            _container.GetMock<IAccountDataStore>().Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme, Amount = expectedBalance };

            var result = _sut.MakePayment(request);

            _container.GetMock<IAccountDataStore>().Verify(m => m.UpdateAccount(It.Is<Account>(a => a.Balance == expectedBalance)));
        }

        [TestCaseSource(nameof(BackupPaymentServiceTestData))]
        public void MakePayment_WithCorrectSettings_UpdatesAccount_OnBackupDataStore(string dataStoreType, PaymentScheme paymentScheme)
        {
            var liveMock = new Mock<IAccountDataStore>();
            var backupMock = new Mock<IAccountDataStore>();
            var testbalance = 1000m;
            var testaccount = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
                Balance = testbalance * 2,
                Status = AccountStatus.Live
            };
            _container.GetMock<IOptionsSnapshot<PaymentServiceOptions>>()
                .SetupGet(m => m.Value).Returns(new PaymentServiceOptions(dataStoreType));
            backupMock.Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme, Amount = testbalance };
            _sut = new PaymentService(_container.GetMock<IOptionsSnapshot<PaymentServiceOptions>>().Object, liveMock.Object, backupMock.Object);

            var result = _sut.MakePayment(request);

            backupMock.Verify(m => m.GetAccount(It.IsAny<string>()), Times.Once);
            backupMock.Verify(m => m.UpdateAccount(It.IsAny<Account>()), Times.Once);
            liveMock.Verify(m => m.GetAccount(It.IsAny<string>()), Times.Never);
            liveMock.Verify(m => m.UpdateAccount(It.IsAny<Account>()), Times.Never);
        }

        [TestCaseSource(nameof(LivePaymentServiceTestData))]
        public void MakePayment_WithCorrectSettings_UpdatesAccount_OnLiveDataStore(string dataStoreType, PaymentScheme paymentScheme)
        {
            var liveMock = new Mock<IAccountDataStore>();
            var backupMock = new Mock<IAccountDataStore>();
            var testbalance = 1000m;
            var testaccount = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
                Balance = testbalance * 2,
                Status = AccountStatus.Live
            };
            _container.GetMock<IOptionsSnapshot<PaymentServiceOptions>>()
                .SetupGet(m => m.Value).Returns(new PaymentServiceOptions(dataStoreType));
            liveMock.Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme, Amount = testbalance };
            _sut = new PaymentService(_container.GetMock<IOptionsSnapshot<PaymentServiceOptions>>().Object, liveMock.Object, backupMock.Object);

            var result = _sut.MakePayment(request);

            backupMock.Verify(m => m.GetAccount(It.IsAny<string>()), Times.Never);
            backupMock.Verify(m => m.UpdateAccount(It.IsAny<Account>()), Times.Never);
            liveMock.Verify(m => m.GetAccount(It.IsAny<string>()), Times.Once);
            liveMock.Verify(m => m.UpdateAccount(It.IsAny<Account>()), Times.Once);
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

        public static IEnumerable<TestCaseData> BackupPaymentServiceTestData()
        {
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.Bacs);
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.FasterPayments);
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.Chaps);
        }

        public static IEnumerable<TestCaseData> LivePaymentServiceTestData()
        {
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.Bacs);
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.FasterPayments);
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.Chaps);
        }
    }
}
