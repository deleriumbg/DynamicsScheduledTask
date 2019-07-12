namespace RegistrationScheduledTasks.Models.Interfaces
{
    public interface IIssueModel
    {
        string AccountName { get; }

        string RegistrationName { get; }

        string IssueDescription { get; }
    }
}
