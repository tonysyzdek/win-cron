using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Retlang.Core;

namespace WinCron.Logging
{
    public class LoggingExecutor : IExecutor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Action<Action, Exception> _onExecuteException;

        public LoggingExecutor(Action<Action, Exception> onExecuteException)
        {
            _onExecuteException = onExecuteException;
        }

        public void Execute(List<Action> toExecute)
        {
            foreach (var action in toExecute)
            {
                Execute(action);
            }
        }

        public void Execute(Action toExecute)
        {
            try
            {
                toExecute();
            }
            catch (Exception e)
            {
                _onExecuteException?.Invoke(toExecute, e);
                Log.Error("Executing action", e);
            }
        }
    }
}
