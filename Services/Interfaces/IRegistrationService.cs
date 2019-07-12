using Xrm;

namespace RegistrationScheduledTasks.Services.Interfaces
{
    public interface IRegistrationService
    {
        bool UpdateRegistration(new_registration registration);
    }
}
