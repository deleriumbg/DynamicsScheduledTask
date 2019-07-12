using System;
using System.Configuration;
using System.Net;
using System.Reflection;
using System.ServiceModel.Description;
using log4net;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using RegistrationScheduledTasks.Core.Interfaces;
using Xrm;

namespace RegistrationScheduledTasks.Core
{
    ///
    /// Singleton class
    ///
    public sealed class CrmConnection : IConnection
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Lazy<CrmConnection> LazyInstance = new Lazy<CrmConnection>(() => new CrmConnection());

        public CrmConnection()
        {
            try
            {
                _log.Info($"Initializing connection...");

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string username = ConfigurationManager.AppSettings["Username"];
                string password = ConfigurationManager.AppSettings["Password"];
                string url = ConfigurationManager.AppSettings["Url"];

                ClientCredentials clientCredentials = new ClientCredentials();
                clientCredentials.UserName.UserName = username;
                clientCredentials.UserName.Password = password;

                OrganizationServiceProxy proxy = new OrganizationServiceProxy(new Uri(url), null, clientCredentials, null);
                proxy.EnableProxyTypes();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(url))
                {
                    _log.Error($"Logging attempt with invalid credentials: Username {username}, Password {password}, Url {url}");
                }
                else
                {
                    _log.Info($"Logging with credentials: Username {username}, Password {password}, Url {url}");
                    OrganizationService = (IOrganizationService)proxy;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Exception caught during connection initialization - {ex.Message}");
            }
        }

        public IOrganizationService OrganizationService { get; }

        public ServiceContext Context => new ServiceContext(OrganizationService);

        public static CrmConnection Instance => LazyInstance.Value;

        public Guid UserId
        {
            get
            {
                WhoAmIRequest systemUserRequest = new WhoAmIRequest();
                WhoAmIResponse systemUserResponse = (WhoAmIResponse)OrganizationService.Execute(systemUserRequest);
                return systemUserResponse.UserId;
            }
        }
    }
}