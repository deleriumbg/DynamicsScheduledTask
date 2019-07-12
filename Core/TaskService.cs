using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using RegistrationScheduledTasks.Core.Interfaces;
using RegistrationScheduledTasks.Models;
using RegistrationScheduledTasks.Models.Interfaces;
using RegistrationScheduledTasks.Services.Interfaces;
using Xrm;

namespace RegistrationScheduledTasks.Core
{
    public class TaskService : ITaskService
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<IRegistrationModel> _registrationsUpdatedList;
        private readonly List<IIssueModel> _issuesList;

        private readonly IRegistrationService _registrationService;
        private readonly ICaseService _caseService;
        private readonly IEmailService _emailService;

        private readonly List<ContactModel> _contactList;
        private readonly List<Incident> _caseList;
        private readonly List<new_registration> _registrationList;
        private readonly IEnumerable<string> _registrationScheduledTaskOwners;
        private readonly Template _template;

        private string _currentRegistrationName;
        private string _currentCaseName;


        public TaskService(IDataAccess dataAccess, IRegistrationService registrationService, 
            ICaseService caseService, IEmailService emailService)
        {
            // Initialize services
            this._registrationService = registrationService;
            this._caseService = caseService;
            this._emailService = emailService;

            this._registrationsUpdatedList = new List<IRegistrationModel>();
            this._issuesList = new List<IIssueModel>();

            // Load data offline
            this._contactList = dataAccess.RetrieveAllContactsWithRelatedAccount();
            this._caseList = dataAccess.RetrieveCancelOneRegistrationCases();
            this._registrationList = dataAccess.RetrieveAllRegistrations();
            this._registrationScheduledTaskOwners = dataAccess.RetrieveEmailAddressesFromSystemRuleValue();
            this._template = dataAccess.GetGlobalTemplateByTitle();
        }

        public void ExecuteTask()
        {
            if (_contactList == null ||_caseList == null || _registrationList == null || _template == null)
            {
                _log.Info($"Exiting {nameof(RegistrationScheduledTasks)}");
                return;
            }

            foreach (var contact in _contactList)
            {
                try
                {
                    // Get cases offline related to the current iteration of contact
                    _log.Info($"Filtering cases related to contact {contact.LastName} with id {contact.Id}");
                    List<Incident> currentAccountCases = _caseList.Where(x => x.CustomerId.Id == contact.Id).ToList();
                    _log.Info($"Retrieved {currentAccountCases.Count} cases for contact id {contact.Id}");

                    foreach (var currentCase in currentAccountCases)
                    {
                        try
                        {
                            // Get registration with the lowest priority, if two registrations found with the same priority take the last created one 
                            _log.Info(
                                $"Filtering registrations related to account {contact.Account.Id} with the lowest priority");
                            new_registration currentRegistration = _registrationList
                                .Where(x => x.new_Account.Id == contact.Account.Id)
                                .OrderBy(p => p.new_Priority)
                                .ThenByDescending(c => c.CreatedOn)
                                .FirstOrDefault();

                            if (currentRegistration == null)
                            {
                                _log.Error(
                                    $"No registration found for account {contact.Account.Name} with id {contact.Account.Id}");
                                continue;
                            }

                            _currentRegistrationName = currentRegistration.new_registrationname;
                            _currentCaseName = currentCase.Title;
                            _log.Info(
                                $"Retrieved registration with name {_currentRegistrationName} for case {_currentCaseName}");

                            bool updateSuccessful = _registrationService.UpdateRegistration(currentRegistration);
                            if (!updateSuccessful)
                            {
                                _issuesList.Add(new IssueModel(contact.Account.Name,
                                    currentRegistration.new_registrationname, "Update was not successful"));
                            }

                            bool caseResolved = _caseService.ResolveCase(currentCase);
                            if (!caseResolved)
                            {
                                _issuesList.Add(new IssueModel(contact.Account.Name,
                                    currentRegistration.new_registrationname, "Case was not resolved successfully"));
                            }

                            bool emailSent = _emailService.CreateEmailFromTemplate(contact, currentCase,
                                currentRegistration.new_registrationname, _template);
                            if (!emailSent)
                            {
                                _issuesList.Add(new IssueModel(contact.Account.Name,
                                    currentRegistration.new_registrationname, "Email not created successfully"));
                            }

                            // Check if there was an issue with this account and if not add it to the successfully updated registrations list
                            if (_issuesList.All(x => x.AccountName != contact.Account.Name))
                            {
                                _registrationsUpdatedList.Add(new RegistrationModel(contact.Account.Name,
                                    _currentRegistrationName, _currentCaseName));
                                
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error(
                                $"Exception caught ïn {nameof(RegistrationScheduledTasks)} for case {currentCase.Id} - {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(
                        $"Exception caught ïn {nameof(RegistrationScheduledTasks)} for contact {contact.Id} - {ex.Message}");
                }
            }

            // Send Registration update summary mail to all task owners
            foreach (var taskOwner in _registrationScheduledTaskOwners)
            {
                bool emailSent = _emailService.SendEmailToRegistrationScheduledTaskOwners(taskOwner, _registrationsUpdatedList,
                    _issuesList);

                if (emailSent)
                {
                    _log.Info($"Registration update summary mail successfully sent to {taskOwner}");
                }
                else
                {
                    _log.Error($"Registration update summary mail was NOT successfully sent to {taskOwner}");
                }
            }
        }
    }
}
