using System.Collections.Generic;
using RegistrationScheduledTasks.Models.Interfaces;
using Xrm;

namespace RegistrationScheduledTasks.Services.Interfaces
{
    public interface IEmailService
    {
        bool CreateEmailFromTemplate(IContactModel contact, Incident incident, string registrationName, Template template);

        bool SendEmailToRegistrationScheduledTaskOwners(string currentScheduledTaskOwner,
            List<IRegistrationModel> registrations, List<IIssueModel> issues);
    }
}
