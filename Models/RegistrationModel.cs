using System.Reflection;
using log4net;
using RegistrationScheduledTasks.Models.Interfaces;

namespace RegistrationScheduledTasks.Models
{
    // Table of registration updated successfully with the following data: Account name, Registration name, Case name
    public class RegistrationModel : IRegistrationModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public RegistrationModel(string accountName, string registrationName, string caseName)
        {
            this.AccountName = accountName;
            this.RegistrationName = registrationName;
            this.CaseName = caseName;
            _log.Info($"Added successfully updated registration: Account name {accountName}, " +
                      $"Registration name {registrationName}, Issue description {caseName}");
        }

        public string AccountName { get; set; }

        public string RegistrationName { get; set; }

        public string CaseName { get; set; }
    }
}
