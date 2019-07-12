using System;
using RegistrationScheduledTasks.Models.Interfaces;

namespace RegistrationScheduledTasks.Models
{
    public class ContactModel : IContactModel
    {
        public Guid? Id { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public AccountModel Account { get; set; }
    }
}
