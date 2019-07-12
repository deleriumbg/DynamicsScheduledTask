using System;

namespace RegistrationScheduledTasks.Models.Interfaces
{
    public interface IAccountModel
    {
        Guid? Id { get; }

        string Name { get; }

        string Email { get; }
    }
}
