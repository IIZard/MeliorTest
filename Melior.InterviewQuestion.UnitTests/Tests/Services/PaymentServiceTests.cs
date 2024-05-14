using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Services;
using Moq.AutoMock;
using NUnit.Framework;

namespace Melior.InterviewQuestion.UnitTests.Tests.Services
{
    [TestFixture]
    public class PaymentServiceTests
    {
        private IPaymentService _sut;
        private AutoMocker _container;

        [SetUp]
        public void SetUp()
        {
            _container = new AutoMocker();
            _sut = _container.CreateInstance<PaymentService>();
        }

        [Test]
        public void MakePayment_OnBackup()
        {
            throw new NotImplementedException();
        }
    }
}
