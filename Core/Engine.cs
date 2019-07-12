using System;
using System.Reflection;
using log4net;
using RegistrationScheduledTasks.Core.Interfaces;


namespace RegistrationScheduledTasks.Core
{
    public class Engine : IEngine
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDataAccess _dataAccess;
        private readonly ITaskService _taskService;

        public Engine(IDataAccess dataAccess, ITaskService taskService)
        {
            this._dataAccess = dataAccess;
            this._taskService = taskService;
        }
        public void Run()
        {
            try
            {
                DateTime? dateToExecute = _dataAccess.RetrieveDateToExecuteFromSystemRule();
                if (dateToExecute == null)
                {
                    _log.Info($"Exiting {nameof(RegistrationScheduledTasks)}");
                    return;
                }

                if (dateToExecute != DateTime.Today)
                {
                    _log.Error($"Date to Execute is different than {DateTime.Today}. Exiting {nameof(RegistrationScheduledTasks)}.");
                    return;
                }

                _taskService.ExecuteTask();

            }
            catch (Exception ex)
            {
                _log.Error($"Exception caught - {ex.Message}");
            }
        }
    }
}
