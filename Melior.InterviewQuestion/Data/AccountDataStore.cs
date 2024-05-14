using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Types;
using System;

namespace Melior.InterviewQuestion.Data
{
    public class AccountDataStore : IAccountDataStore
    {
        public Account GetAccount(string accountNumber)
        {
            // Access database to retrieve account, code removed for brevity 
            throw new NotImplementedException();
        }

        public void UpdateAccount(Account account)
        {
            // Update account in database, code removed for brevity
            throw new NotImplementedException();
        }
    }
}
