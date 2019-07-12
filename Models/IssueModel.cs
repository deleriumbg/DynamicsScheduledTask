using System.Reflection;
using log4net;
using RegistrationScheduledTasks.Models.Interfaces;

namespace RegistrationScheduledTasks.Models
{
    // Table of issues with the following data: Account name, Registration name, Issue description 
    public class IssueModel : IIssueModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IssueModel(string accountName, string registrationName, string issueDescription)
        {
            this.AccountName = accountName;
            this.RegistrationName = registrationName;
            this.IssueDescription = issueDescription;
            _log.Info($"Added issue: Account name {accountName}, " +
                      $"Registration name {registrationName}, Issue description {issueDescription}");
        }

        public string AccountName { get; set; }

        public string RegistrationName { get; set; }

        public string IssueDescription { get; set; }
    }
}
