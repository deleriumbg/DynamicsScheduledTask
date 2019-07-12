using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using log4net;
using RegistrationScheduledTasks.Core.Interfaces;
using RegistrationScheduledTasks.Models;
using Xrm;

namespace RegistrationScheduledTasks.Core
{
    public class DataAccess : IDataAccess
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EmailTemplateGuid = "42AAEEC0-DEA3-E911-A980-000D3A26C11D";
        private const string TemplateTitle = "Case Resolve: Cancel Registration";
        private readonly Guid _templateId = Guid.Parse(EmailTemplateGuid);
        private readonly ServiceContext _context;

        public DataAccess(IConnection connection)
        {
            this._context = connection.Context;
        }

        public DateTime? RetrieveDateToExecuteFromSystemRule()
        {
            try
            {
                using (var context = _context)
                {
                    _log.Info("Retrieving Date to run registration scheduled task System rule value");
                    string dateToExecuteString = (from date in context.new_systemrulesSet
                                                  where date.new_Slug.Equals("date_to_run_registration_scheduled_task")
                                                  select date.new_RuleValue)
                                                  .FirstOrDefault();

                    if (dateToExecuteString == null)
                    {
                        _log.Error("Date to Execute field is empty");
                        return null;
                    }

                    _log.Info($"Retrieved System rule value {dateToExecuteString}. Converting it to DateTime...");
                    var cultureInfo = CultureInfo.CurrentCulture;
                    var formats = new[] { "M-d-yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "M.d.yyyy", "dd.MM.yyyy", "MM.dd.yyyy" }
                        .Union(cultureInfo.DateTimeFormat.GetAllDateTimePatterns()).ToArray();

                    DateTime dateToExecute;
                    try
                    {
                        dateToExecute = DateTime.ParseExact(dateToExecuteString, formats, cultureInfo,
                            DateTimeStyles.AssumeLocal);
                        _log.Info($"Successfully parsed to date {dateToExecuteString}");
                    }
                    catch (FormatException)
                    {
                        _log.Error(
                            $"Unsupported time format in Date to run registration scheduled task System rule value - {dateToExecuteString}");
                        return null;
                    }

                    return dateToExecute;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception caught during retrieving date from Date to run registration scheduled task System rule value - {ex.Message}");
                return null;
            }
        }

        public IEnumerable<string> RetrieveEmailAddressesFromSystemRuleValue()
        {
            try
            {
                using (var context = _context)
                {
                    _log.Info($"Retrieving email addresses from Registration scheduled task owners System rule value...");
                    var scheduledTaskOwnersEmails = (from emails in context.new_systemrulesSet
                                                     where emails.new_Slug.Equals("registration_scheduled_task_owners")
                                                     select emails.new_RuleValue)
                                                     .FirstOrDefault();
                    if (scheduledTaskOwnersEmails == null)
                    {
                        _log.Error("Registration scheduled task owners System rule value is empty");
                        return null;
                    }

                    var emailList = scheduledTaskOwnersEmails
                        .Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    _log.Info($"Retrieved {emailList.Count} emails from Registration scheduled task owners System rule value");
                    return emailList;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception caught during retrieving email addresses from Registration scheduled task owners System rule value - {ex.Message}");
                return null;
            }
        }

        public List<ContactModel> RetrieveAllContactsWithRelatedAccount()
        {
            try
            {
                using (var context = _context)
                {
                    _log.Info($"Retrieving All contacts with related account...");
                    var contacts = (from account in context.AccountSet
                                    join contact in context.ContactSet on account.PrimaryContactId.Id equals contact.ContactId
                                    join registration in context.new_registrationSet on account.AccountId equals registration.new_Account.Id
                                    select new ContactModel
                                    {
                                        Id = contact.ContactId,
                                        LastName = contact.LastName,
                                        Email = contact.EMailAddress1,
                                        Account = new AccountModel
                                        {
                                            Id = account.AccountId,
                                            Name = account.Name,
                                            Email = account.EMailAddress1
                                        }
                                    })
                                    .Distinct()
                                    .ToList();

                    _log.Info($"Retrieved {contacts.Count} contacts with related account");
                    return contacts;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception caught during retrieving All contacts with related account - {ex.Message}");
                return null;
            }
            
        }

        public List<Incident> RetrieveCancelOneRegistrationCases()
        {
            try
            {
                using (var context = _context)
                {
                    _log.Info($"Retrieving Cancel one registration cases...");
                    var cases = (from incidents in context.IncidentSet
                                 where incidents.Title.Equals("Cancel one registration") &&
                                       incidents.CaseTypeCode.Value == 3 // Request
                                 select incidents)
                                 .ToList();

                    if (cases.Count == 0)
                    {
                        _log.Info($"No Cancel one registration cases found.");
                        return null;
                    }
                    _log.Info($"Retrieved {cases.Count} Cancel one registration cases.");
                    return cases;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception caught during retrieving Cancel one registration cases - {ex.Message}");
                return null;
            }
            
        }

        public List<new_registration> RetrieveAllRegistrations()
        {
            try
            {
                using (var context = _context)
                {
                    _log.Info($"Retrieving all registrations with status Open and sub status Registered...");
                    var registrations = (from registration in context.new_registrationSet
                                         join status in context.new_registrationstatusSet on registration.new_RegistrationStatus.Id equals status.Id
                                         join subStatus in context.new_registrationsubstatusSet on registration.new_RegistrationSubStatus.Id equals subStatus.Id
                                         where registration.new_Account != null && 
                                               status.new_name.Equals("Open") &&
                                               subStatus.new_name.Equals("Registered")
                                         select registration)
                                         .ToList();

                    if (registrations.Count == 0)
                    {
                        _log.Info($"No registrations found with status Open and sub status Registered.");
                        return null;
                    }
                    _log.Info($"Retrieved {registrations.Count} registrations with status Open and sub status Registered.");
                    return registrations;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception caught during retrieving all registrations with status Open and sub status Registered - {ex.Message}");
                return null;
            }
        }

        public Template GetGlobalTemplateById()
        {
            try
            {
                _log.Info($"Retrieving template by id {_templateId}...");
                using (var context = _context)
                {
                    var template = (from templates in context.TemplateSet
                                    where templates.Id == _templateId
                                    select templates)
                                    .FirstOrDefault();

                    if (template == null)
                    {
                        _log.Error($"No template found with id {_templateId}");
                        return null;
                    }

                    _log.Info($"Successfully retrieved template {template.Title} with id {_templateId}");
                    return template;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception caught during retrieving of global template: {_templateId} by id - {ex.Message}");
                return null;
            }
        }

        public Template GetGlobalTemplateByTitle()
        {
            try
            {
                _log.Info($"Retrieving template by title {TemplateTitle}...");
                using (var context = _context)
                {
                    var template = (from templates in context.TemplateSet
                                    where templates.Title.Equals(TemplateTitle)
                                    select templates)
                                    .FirstOrDefault();

                    if (template == null)
                    {
                        _log.Error($"No template found with title {TemplateTitle}");
                        return null;
                    }

                    _log.Info($"Successfully retrieved template {TemplateTitle} with id {template.Id}");
                    return template;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception caught during retrieving of global template: {_templateId} by title - {ex.Message}");
                return null;
            }
        }
    }
}
