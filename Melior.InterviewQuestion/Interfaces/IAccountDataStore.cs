using Melior.InterviewQuestion.Types;

namespace Melior.InterviewQuestion.Interfaces
{
    public interface IAccountDataStore
    {
        Account GetAccount(string accountNumber);
        void UpdateAccount(Account account);
    }
}