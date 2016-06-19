using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;

namespace StockMan.Jobs.Listeners
{
    public class GlobalTriggerListener : ITriggerListener
    {
        public string Name
        {
            get { return "GlobalTriggerListener"; }
        }

        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            this.Log().Info("Trigger完成：" + trigger.Description);
        }

        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            this.Log().Info("Trigger被触发：" + trigger.Description);
        }

        public void TriggerMisfired(ITrigger trigger)
        {
            this.Log().Info("Trigger未被触发：" + trigger.Description);
        }

        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            this.Log().Info("Trigger否决job：" + trigger.Description);
            return false;
        }



    }
}