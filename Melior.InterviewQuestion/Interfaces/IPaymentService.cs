using Melior.InterviewQuestion.Types;

namespace Melior.InterviewQuestion.Interfaces
{
    public interface IPaymentService
    {
        MakePaymentResult MakePayment(MakePaymentRequest request);
    }
}
