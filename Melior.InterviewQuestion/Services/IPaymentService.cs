using Melior.InterviewQuestion.Types;

namespace Melior.InterviewQuestion.Services
{
    public interface IPaymentService
    {
        MakePaymentResult MakePayment(MakePaymentRequest request);
    }
}
