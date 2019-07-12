using System;
using System.Reflection;
using log4net;
using Microsoft.Xrm.Sdk;
using RegistrationScheduledTasks.Core.Interfaces;
using RegistrationScheduledTasks.Services.Interfaces;
using Xrm;

namespace RegistrationScheduledTasks.Services
{
    public class RegistrationService : IRegistrationService
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string RegistrationStatusCancelled = "0A998C04-7BA1-E911-A980-000D3A26C11D";
        private const string RegistrationSubstatusCancelledByTask = "F5420F1D-7BA1-E911-A980-000D3A26C11D";
        private readonly IOrganizationService _service;
        private readonly Guid _userId;

        public RegistrationService(IConnection connection)
        {
            this._service = connection.OrganizationService;
            this._userId = connection.UserId;
        }

        public bool UpdateRegistration(new_registration registration)
        {
            try
            {
                // Creating new registration object for update
                _log.Info($"Updating registration {registration.Id}...");
                var registrationToUpdate = new new_registration
                {
                    Id = registration.Id,
                    new_RegistrationStatus = new EntityReference(new_registrationstatus.EntityLogicalName,
                        Guid.Parse(RegistrationStatusCancelled)),
                    new_RegistrationSubStatus = new EntityReference(new_registrationsubstatus.EntityLogicalName,
                        Guid.Parse(RegistrationSubstatusCancelledByTask)),
                    new_ModifiedBy = new EntityReference(SystemUser.EntityLogicalName, _userId),
                    new_ModifiedOn = DateTime.Now
                };

                _service.Update(registrationToUpdate);
                _log.Info($"Successfully updated registration {registration.Id} with the following values: " +
                          $"Status {registrationToUpdate.new_RegistrationStatus.Name}, " +
                          $"SubStatus {registrationToUpdate.new_RegistrationSubStatus.Name}, " +
                          $"Modified By {registrationToUpdate.new_ModifiedBy.Name}, " +
                          $"Modified On {DateTime.Now}");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"Exception caught during updating registration {registration.Id} - {ex.Message}");
                return false;
            }
        }
    }
}
