using System;
using System.Collections.Generic;

namespace CitySim.Data
{
    public class Wallet
    {
        public decimal Balance { get; set; }
        public decimal Overdraft { get; set; }
        public List<ScheduledTransaction> Credits { get; set; } = [];
        public List<ScheduledTransaction> Debits { get; set; } = [];

        public bool Debit(decimal amount)
        {
            if (Balance + Overdraft < amount) return false;

            Balance -= amount;
            return true;
        }

        public void Credit(decimal amount) => Balance += amount;
    }

    public class ScheduledTransaction
    {
        public decimal Amount { get; set; }
        public int DayOfMonth { get; set; } = 1;
        public DateTime? LastTransactionDate { get; set; }
    }
}
