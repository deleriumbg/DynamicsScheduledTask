using System;
using System.Reflection;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using RegistrationScheduledTasks.Core;
using RegistrationScheduledTasks.Core.Interfaces;
using RegistrationScheduledTasks.Services;
using RegistrationScheduledTasks.Services.Interfaces;


namespace RegistrationScheduledTasks
{
    public class StartUp
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            _log.Info($"Start executing {nameof(RegistrationScheduledTasks)}...");
            IServiceCollection serviceCollection = CreateCollection();
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IEngine engine = serviceProvider.GetService<IEngine>();
            engine.Run();
            _log.Info($"Finished executing {nameof(RegistrationScheduledTasks)}...");
        }

        private static IServiceCollection CreateCollection()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            // Registered singleton CRM connection
            serviceCollection.AddSingleton<IConnection, CrmConnection>();

            // Register utilities services
            serviceCollection.AddScoped<ICaseService, CaseService>();
            serviceCollection.AddScoped<IEmailService, EmailService>();
            serviceCollection.AddScoped<IRegistrationService, RegistrationService>();

            // Register Buisness logic services
            serviceCollection.AddTransient<IEngine, Engine>();
            serviceCollection.AddTransient<IDataAccess, DataAccess>();
            serviceCollection.AddTransient<ITaskService, TaskService>();

            _log.Info("Service collection created successfully");
            return serviceCollection;
        }
    }
}
