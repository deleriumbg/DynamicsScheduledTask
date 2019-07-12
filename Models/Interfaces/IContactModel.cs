using System;

namespace RegistrationScheduledTasks.Models.Interfaces
{
    public interface IContactModel
    {
        Guid? Id { get; }

        string LastName { get; }

        string Email { get; }

        AccountModel Account { get; }
    }
}
