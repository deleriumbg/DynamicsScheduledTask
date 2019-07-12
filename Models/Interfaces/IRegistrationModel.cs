namespace RegistrationScheduledTasks.Models.Interfaces
{
    public interface IRegistrationModel
    {
        string AccountName { get; }

        string RegistrationName { get; }

        string CaseName { get; }
    }
}
