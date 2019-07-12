using Xrm;

namespace RegistrationScheduledTasks.Services.Interfaces
{
    public interface ICaseService
    {
        bool ResolveCase(Incident incident);
    }
}
