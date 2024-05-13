using System;
using System.Collections.Generic;

namespace Primo.RobotStateKeeper
{
    public class RobotStateModel
    {   
        public bool CompletedSeccessfully { get; set; } = false;
        public string LastTransactionId { get; set; }
        public string LastTransactionStatus { get; set; }        
        public List<RobotErrors> RobotsErrors { get; set; } = new List<RobotErrors>();
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public void UpdateLastTransactionId(string transactionId)
        {
            LastTransactionId = transactionId;
        }
    }
    public class RobotErrors
    {
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string PathOfTheSequence { get; set; }
        public string Block { get; set; }

        public delegate void TransactionIdChangedHandler(string newTransactionId);
        public event TransactionIdChangedHandler OnTransactionIdChanged;

        private string _transactionItemId;
        public string TransactionItemId
        {
            get => _transactionItemId;
            set
            {
                _transactionItemId = value;
                IsTransaction = !string.IsNullOrEmpty(_transactionItemId);
                OnTransactionIdChanged?.Invoke(_transactionItemId);
            }
        }
        public bool IsNotyficationSend { get; set; } = false;
        public bool IsTransaction { get; set; } = false;
        public int TrysToRepead { get; set; } = 0;
        public DateTime TimeStamp { get; set; } = DateTime.Now;
    }
}
