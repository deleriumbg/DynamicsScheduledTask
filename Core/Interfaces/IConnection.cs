using System;
using Microsoft.Xrm.Sdk;
using Xrm;

namespace RegistrationScheduledTasks.Core.Interfaces
{
    public interface IConnection
    {
        IOrganizationService OrganizationService { get; }

        ServiceContext Context { get; }

        Guid UserId { get; }
    }
}