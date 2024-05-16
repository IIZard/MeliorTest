using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Services;
using Melior.InterviewQuestion.Types;
using Microsoft.Extensions.Options;
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

        [TestCaseSource(nameof(PaymentServiceTestData))]
        public void MakePayment_WithNullAccount_IsNotSuccess(string dataStoreType, PaymentScheme paymentScheme)
        {
            _container.GetMock<IOptionsSnapshot<PaymentServiceOptions>>()
                .SetupGet(m => m.Value).Returns(new PaymentServiceOptions(dataStoreType));
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme };

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.False);
        }

        public static IEnumerable<TestCaseData> PaymentServiceTestData()
        {
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.Bacs);
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.FasterPayments);
            yield return new TestCaseData(BackupDataStoreName, PaymentScheme.Chaps);
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.Bacs);
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.FasterPayments);
            yield return new TestCaseData(LiveDataStoreName, PaymentScheme.Chaps);
        }
    }
}
