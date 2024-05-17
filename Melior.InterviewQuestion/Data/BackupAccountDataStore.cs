using Melior.InterviewQuestion.Interfaces;
using Melior.InterviewQuestion.Types;
using System;

namespace Melior.InterviewQuestion.Data
{
    public class BackupAccountDataStore : IAccountDataStore
    {
        public Account GetAccount(string accountNumber)
        {
            // Access backup data base to retrieve account, code removed for brevity 
            throw new NotImplementedException();
        }

        public void UpdateAccount(Account account)
        {
            // Update account in backup database, code removed for brevity
            throw new NotImplementedException();
        }
    }
}
