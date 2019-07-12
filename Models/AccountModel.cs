using System;
using RegistrationScheduledTasks.Models.Interfaces;

namespace RegistrationScheduledTasks.Models
{
    public class AccountModel : IAccountModel
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }
    }
}
