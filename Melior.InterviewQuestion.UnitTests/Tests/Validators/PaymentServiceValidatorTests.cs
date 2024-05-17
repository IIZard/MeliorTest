using Melior.InterviewQuestion.Types;
using Melior.InterviewQuestion.Validators;
using NUnit.Framework;

namespace Melior.InterviewQuestion.UnitTests.Tests.Validators
{
    [TestFixture]
    public class PaymentServiceValidatorTests
    {
        private PaymentServiceValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new PaymentServiceValidator();
        }

        [Test]
        public void AccountMustNotBeNull()
        {
            Assert.That(_sut.Validate(new PaymentServiceValidatorContext((Account)null, new MakePaymentRequest())).IsValid, Is.False);
        }

        [TestCaseSource(nameof(AccountPaymentSchemeData))]
        public void AccountMustHaveAllowedPaymentScheme(AllowedPaymentSchemes allowedPaymentSchemes, PaymentScheme ps, bool expectation)
        {
            var testacc = new Account { AllowedPaymentSchemes = allowedPaymentSchemes, Status = AccountStatus.Live, Balance = 1000 };
            var testreq = new MakePaymentRequest { PaymentScheme = ps, Amount = 500 };
            Assert.That(_sut.Validate(new PaymentServiceValidatorContext(testacc, testreq)).IsValid, Is.EqualTo(expectation));
        }

        [TestCaseSource(nameof(FasterPaymentsMustHaveEnoughBalanceData))]
        public void FasterPayments_MustHaveEnoughBalance(decimal request, decimal balance, bool expectation)
        {
            var testacc = new Account { AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments, Status = AccountStatus.Live, Balance = balance };
            var testreq = new MakePaymentRequest { PaymentScheme = PaymentScheme.FasterPayments, Amount = request };
            Assert.That(_sut.Validate(new PaymentServiceValidatorContext(testacc, testreq)).IsValid, Is.EqualTo(expectation));
        }

        [TestCaseSource(nameof(ChapsMustBeLiveData))]
        public void Chaps_MustBeLive(AccountStatus status, bool expectation)
        {
            var testacc = new Account { AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps, Status = status };
            var testreq = new MakePaymentRequest { PaymentScheme = PaymentScheme.Chaps };
            Assert.That(_sut.Validate(new PaymentServiceValidatorContext(testacc, testreq)).IsValid, Is.EqualTo(expectation));
        }

        public static IEnumerable<TestCaseData> AccountPaymentSchemeData()
        {
            yield return new TestCaseData(AllowedPaymentSchemes.Bacs, PaymentScheme.FasterPayments, false);
            yield return new TestCaseData(AllowedPaymentSchemes.Bacs, PaymentScheme.Chaps, false);
            yield return new TestCaseData(AllowedPaymentSchemes.FasterPayments, PaymentScheme.Chaps, false);
            yield return new TestCaseData(AllowedPaymentSchemes.FasterPayments, PaymentScheme.Bacs, false);
            yield return new TestCaseData(AllowedPaymentSchemes.Chaps, PaymentScheme.FasterPayments, false);
            yield return new TestCaseData(AllowedPaymentSchemes.Chaps, PaymentScheme.Bacs, false);

            yield return new TestCaseData(AllowedPaymentSchemes.Bacs, PaymentScheme.Bacs, true);
            yield return new TestCaseData(AllowedPaymentSchemes.FasterPayments, PaymentScheme.FasterPayments, true);
            yield return new TestCaseData(AllowedPaymentSchemes.Chaps, PaymentScheme.Chaps, true);
        }

        public static IEnumerable<TestCaseData> FasterPaymentsMustHaveEnoughBalanceData()
        {
            yield return new TestCaseData(500m, 499m, false);
            yield return new TestCaseData(500m, 500m, false); // intended?
            yield return new TestCaseData(500m, 501m, true);
        }
        public static IEnumerable<TestCaseData> ChapsMustBeLiveData()
        {
            yield return new TestCaseData(AccountStatus.InboundPaymentsOnly, false);
            yield return new TestCaseData(AccountStatus.Disabled, false);
            yield return new TestCaseData(AccountStatus.Live, true);
        }

    }
}
