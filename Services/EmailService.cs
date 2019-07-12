using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using log4net;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using RegistrationScheduledTasks.Core.Interfaces;
using RegistrationScheduledTasks.Models.Interfaces;
using RegistrationScheduledTasks.Services.Interfaces;
using Xrm;

namespace RegistrationScheduledTasks.Services
{
    public class EmailService : IEmailService
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string TableRowStyle =
            @"<td style=""border: 1px solid #dddddd; text-align: left; padding: 8px;"">";
        private readonly IOrganizationService _service;
        private readonly Guid _userId;
        private Guid _contactId;

        public EmailService(IConnection connection)
        {
            this._service = connection.OrganizationService;
            this._userId = connection.UserId;
        }

        private string GetDataFromXml(string value, string attributeName)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            XDocument document = XDocument.Parse(value);

            // Get the Element with the attribute name specified
            XElement element = document.Descendants().FirstOrDefault(ele => ele.Attributes()
                .Any<XAttribute>(attr => attr.Name == attributeName));
            return element == null ? string.Empty : element.Value;
        }

        private bool SendEmailFromTemplate(Email email, Guid templateId)
        {
            try
            {
                SendEmailFromTemplateRequest request = new SendEmailFromTemplateRequest
                {
                    Target = email,
                    TemplateId = templateId,
                    RegardingId = _contactId,
                    RegardingType = Contact.EntityLogicalName
                };
                SendEmailFromTemplateResponse response = (SendEmailFromTemplateResponse)_service.Execute(request);
                return response != null;
            }
            catch (Exception ex)
            {
                _log.Error($"An error occurred during sending email. Exception: {ex.Message}");
                return false;
            }
        }

        private string BuildDescription(List<IRegistrationModel> registrations, List<IIssueModel> issues)
        {
            string html = File.ReadAllText("../../Resources/SuccessfulRegistrationsTable.html");
            foreach (var registration in registrations)
            {
                html += @"<tr>" +
                            TableRowStyle + registration.AccountName + @"</td>" +
                            TableRowStyle + registration.RegistrationName + @"</td>" +
                            TableRowStyle + registration.CaseName + @"</td>" +
                        "</tr>";
            }

            html += "</table>";
            html = string.Format(html, registrations.Count);

            string issueHtml = File.ReadAllText("../../Resources/IssuesTable.html");
            html += issueHtml;
            foreach (var issue in issues)
            {
                html += @"<tr>" +
                            TableRowStyle + issue.AccountName + @"</td>" +
                            TableRowStyle + issue.RegistrationName + @"</td>" +
                            TableRowStyle + issue.IssueDescription + @"</td>" +
                        "</tr>";
            }
            html += "</table>";
            html = string.Format(html, issues.Count);
            return html;
        }

        public bool CreateEmailFromTemplate(IContactModel contact, Incident incident, string registrationName, Template template)
        {
            try
            {
                _log.Info($"Creating email message from template {template.Title}...");
                _contactId = contact.Id ?? Guid.Empty;

                // Create the 'From:' activity party for the email
                ActivityParty fromParty = new ActivityParty
                {
                    PartyId = new EntityReference(SystemUser.EntityLogicalName, _userId)
                };

                // Create the 'To:' activity party for the email
                ActivityParty toParty = new ActivityParty
                {
                    PartyId = new EntityReference(Contact.EntityLogicalName, _contactId),
                    AddressUsed = contact.Email
                };
                _log.Info("Created activity parties.");

                // Create an e-mail message.
                Email email = new Email
                {
                    To = new ActivityParty[] { toParty },
                    From = new ActivityParty[] { fromParty },
                    Subject = GetDataFromXml(template.Subject, "match"),
                    Description = GetDataFromXml(template.Body, "match"),
                    DirectionCode = true,
                    RegardingObjectId = new EntityReference(Contact.EntityLogicalName, _contactId)
                };
                
                _log.Info("Start modifying the email description with dynamic values...");
                email.Description = email.Description.Replace("#LastName#", contact.LastName);
                email.Description = email.Description.Replace("#caseNumber#", incident.TicketNumber);
                email.Description = email.Description.Replace("#RegistrationName#", registrationName);

                Guid emailId = _service.Create(email);
                if (emailId == Guid.Empty)
                {
                    _log.Error("Email not sent successfully");
                    return false;
                }
                _log.Info($"Created email message with id {emailId} with the following information: " +
                          $"Template {template.Title}, Subject {email.Subject}, Description {email.Description}");
                return true;
                //bool emailSent = SendEmailFromTemplate(email, template.Id);

            }
            catch (Exception ex)
            {
                _log.Error($"An error occurred during creating email. Exception: {ex.Message}");
                return false;
            }
        }

        public bool SendEmailToRegistrationScheduledTaskOwners(string currentScheduledTaskOwner, List<IRegistrationModel> registrations, List<IIssueModel> issues)
        {
            _log.Info($"Creating email message to Scheduled task owner {currentScheduledTaskOwner}...");

            // Create the 'From:' activity party for the email
            ActivityParty fromParty = new ActivityParty
            {
                PartyId = new EntityReference(SystemUser.EntityLogicalName, _userId)
            };

            // Create the 'To:' activity party for the email
            ActivityParty toParty = new ActivityParty
            {
                AddressUsed = currentScheduledTaskOwner
            };

            _log.Info("Created To and From activity parties.");

            string description = BuildDescription(registrations, issues);

            // Create an email message entity
            Email email = new Email
            {
                To = new ActivityParty[] { toParty },
                From = new ActivityParty[] { fromParty },
                Subject = "Registration Update Summary",
                Description = description,
                DirectionCode = true
            };

            Guid emailId = _service.Create(email);
            if (emailId == Guid.Empty)
            {
                _log.Error("Email not sent successfully");
                return false;
            }
            _log.Info($"Created email message with id {emailId} with the following information: " +
                      $"Subject {email.Subject}, Description {email.Description}");

            SendEmailRequest sendEmailRequest = new SendEmailRequest
            {
                EmailId = emailId,
                TrackingToken = string.Empty,
                IssueSend = true
            };

            SendEmailResponse sendEmailResponse = (SendEmailResponse)_service.Execute(sendEmailRequest);
            return sendEmailResponse != null;
        }
    }
}
