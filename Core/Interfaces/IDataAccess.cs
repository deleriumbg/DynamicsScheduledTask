using System;
using System.Collections.Generic;
using RegistrationScheduledTasks.Models;
using Xrm;

namespace RegistrationScheduledTasks.Core.Interfaces
{
    public interface IDataAccess
    {
        DateTime? RetrieveDateToExecuteFromSystemRule();

        List<ContactModel> RetrieveAllContactsWithRelatedAccount();

        List<Incident> RetrieveCancelOneRegistrationCases();

        List<new_registration> RetrieveAllRegistrations();

        Template GetGlobalTemplateById();

        IEnumerable<string> RetrieveEmailAddressesFromSystemRuleValue();

        Template GetGlobalTemplateByTitle();
    }
}
