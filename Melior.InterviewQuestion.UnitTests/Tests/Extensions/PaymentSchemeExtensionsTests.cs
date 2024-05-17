using Melior.InterviewQuestion.Extensions;
using Melior.InterviewQuestion.Types;
using NUnit.Framework;

namespace Melior.InterviewQuestion.UnitTests.Tests.Extensions
{
    [TestFixture]
    public class PaymentSchemeExtensionsTests
    {
        [TestCaseSource(nameof(ConvertToAlowedPaymentSchemeData))]
        public void ConvertToAlowedPaymentScheme_ConvertsAsExpected(PaymentScheme paymentScheme, AllowedPaymentSchemes expectedAllowedPaymentScheme)
        {
            Assert.That(paymentScheme.ConvertToAlowedPaymentScheme(), Is.EqualTo(expectedAllowedPaymentScheme));
        }

        public static IEnumerable<TestCaseData> ConvertToAlowedPaymentSchemeData()
        {
            yield return new TestCaseData(PaymentScheme.Bacs, AllowedPaymentSchemes.Bacs);
            yield return new TestCaseData(PaymentScheme.FasterPayments, AllowedPaymentSchemes.FasterPayments);
            yield return new TestCaseData(PaymentScheme.Chaps, AllowedPaymentSchemes.Chaps);
            yield return new TestCaseData(50, 0);
        }
    }
}
