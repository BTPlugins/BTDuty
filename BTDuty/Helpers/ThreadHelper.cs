using Rocket.Core.Logging;
using Rocket.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BTDuty.Helpers
{
    public class ThreadHelper
    {
        public static void RunAsynchronously(Action action, string exceptionMessage = null)
        {
            ThreadPool.QueueUserWorkItem((_) =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    RunSynchronously(() => Logger.LogException(e, exceptionMessage));
                }
            });
        }

        public static void RunSynchronously(Action action, float delaySeconds = 0)
        {
            TaskDispatcher.QueueOnMainThread(action, delaySeconds);
        }
    }
}
