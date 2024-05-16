﻿using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Services;
using Melior.InterviewQuestion.Types;
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
        public void MakePayment_WithNullAccount_IsNotSuccess(PaymentScheme paymentScheme)
        {
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme };

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.False);
        }

        [TestCaseSource(nameof(AllPaymentServiceTestData))]
        public void MakePayment_GetsDebtorAccountNumber(PaymentScheme paymentScheme)
        {
            var expectedAccountNumber = "1234";
            var testaccount = new Account { AllowedPaymentSchemes = 0 };
            _container.GetMock<IAccountDataStore>().Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme, DebtorAccountNumber = expectedAccountNumber };

            var result = _sut.MakePayment(request);

            _container.GetMock<IAccountDataStore>().Verify(m => m.GetAccount(expectedAccountNumber), Times.Once);
        }

        [TestCaseSource(nameof(AllPaymentServiceTestData))]
        public void MakePayment_WithNoAllowedPaymentScheme_IsNotSuccess(PaymentScheme paymentScheme)
        {
            var testaccount = new Account { AllowedPaymentSchemes = 0 };
            _container.GetMock<IAccountDataStore>().Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme };

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.False);
        }

        [TestCaseSource(nameof(FasterPaymentsPaymentServiceTestData))]
        public void MakePayment_FasterPayments_WithLowBalance_IsNotSuccess(PaymentScheme paymentScheme)
        {
            var testbalance = 1000m;
            var testaccount = new Account { AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments, Balance = testbalance };
            _container.GetMock<IAccountDataStore>().Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme, Amount = testbalance * 2 };

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.False);
        }

        [TestCaseSource(nameof(ChapsPaymentServiceTestData))]
        public void MakePayment_Chaps_WithoutLiveStatus_IsNotSuccess(PaymentScheme paymentScheme)
        {
            var testaccount = new Account { AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps, Status = AccountStatus.Disabled };
            _container.GetMock<IAccountDataStore>().Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme };

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.False);
        }

        [TestCaseSource(nameof(AllPaymentServiceTestData))]
        public void MakePayment_WithCorrectSettings_IsSuccess(PaymentScheme paymentScheme)
        {
            var testbalance = 1000m;
            var testaccount = new Account { 
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps, 
                Balance = testbalance * 2,
                Status = AccountStatus.Live
            };
            _container.GetMock<IAccountDataStore>().Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme, Amount = testbalance };

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.True);
        }

        [TestCaseSource(nameof(AllPaymentServiceTestData))]
        public void MakePayment_WithCorrectSettings_UpdatesAccount_WithNewBalance(PaymentScheme paymentScheme)
        {
            var expectedBalance = 1000m;
            var testaccount = new Account
            {
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
                Balance = expectedBalance * 2,
                Status = AccountStatus.Live
            };
            _container.GetMock<IAccountDataStore>().Setup(m => m.GetAccount(It.IsAny<string>()))
                .Returns(testaccount);
            var request = new MakePaymentRequest { PaymentScheme = paymentScheme, Amount = expectedBalance };

            var result = _sut.MakePayment(request);

            _container.GetMock<IAccountDataStore>().Verify(m => m.UpdateAccount(It.Is<Account>(a => a.Balance == expectedBalance)));
        }

        public static IEnumerable<TestCaseData> AllPaymentServiceTestData()
        {
            yield return new TestCaseData(PaymentScheme.Bacs);
            yield return new TestCaseData(PaymentScheme.FasterPayments);
            yield return new TestCaseData(PaymentScheme.Chaps);
        }

        public static IEnumerable<TestCaseData> FasterPaymentsPaymentServiceTestData()
        {
            yield return new TestCaseData(PaymentScheme.FasterPayments);
        }

        public static IEnumerable<TestCaseData> ChapsPaymentServiceTestData()
        {
            yield return new TestCaseData(PaymentScheme.Chaps);
        }
    }
}
