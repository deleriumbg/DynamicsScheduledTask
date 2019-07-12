using System;
using System.Reflection;
using log4net;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using RegistrationScheduledTasks.Core.Interfaces;
using RegistrationScheduledTasks.Services.Interfaces;
using Xrm;

namespace RegistrationScheduledTasks.Services
{
    public class CaseService : ICaseService
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IOrganizationService _service;

        public CaseService(IConnection connection)
        {
            this._service = connection.OrganizationService;
        }

        public bool ResolveCase(Incident incident)
        {
            try
            {
                //Create Incident Resolution
                var incidentResolution = new IncidentResolution
                {
                    Subject = "Case Resolved",
                    IncidentId = new EntityReference(Incident.EntityLogicalName, incident.Id),
                    ActualEnd = DateTime.Now
                };

                //Close Incident
                var closeIncidenRequst = new CloseIncidentRequest
                {
                    IncidentResolution = incidentResolution,
                    Status = new OptionSetValue(5)
                };

                _service.Execute(closeIncidenRequst);
                _log.Info($"Successfully resolved case {incident.TicketNumber} with id {incident.Id}");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"Exception caught during resolving case {incident.TicketNumber} with id {incident.Id} - {ex.Message}");
                return false;
            }
        }
    }
}
