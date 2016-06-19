using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;

namespace StockMan.Jobs.Listeners
{
    public class SchedulerListener : ISchedulerListener
    {
        //public void JobScheduled(ITrigger trigger)
        //{
        //    this.Log().Info("Job调度，触发器："+trigger.Description);
        //}

        //public void JobUnscheduled(string triggerName, string triggerGroup)
        //{
        //    this.Log().Info("Job卸载，触发器：" + triggerName);
        //}

        //public void JobsPaused(string jobName, string jobGroup)
        //{
        //    this.Log().Info("Job暂停：" + jobName);
        //}

        //public void JobsResumed(string jobName, string jobGroup)
        //{
        //    this.Log().Info("Job重新启动：" + jobName);
        //}

        //public void SchedulerError(string msg, SchedulerException cause)
        //{
        //    this.Log().Info("Job异常：" + msg+cause.Message);
        //}

        //public void SchedulerShutdown()
        //{
        //    this.Log().Info("调度器关闭:"+DateTime.Now.ToString());
        //}

        //public void TriggerFinalized(ITrigger trigger)
        //{
        //    this.Log().Info("触发器终止:" + trigger.Description);
        //}

        //public void TriggersPaused(string triggerName, string triggerGroup)
        //{
        //    this.Log().Info("触发器暂停:" + triggerName);
        //}

        //public void TriggersResumed(string triggerName, string triggerGroup)
        //{
        //    this.Log().Info("触发器重启:" + triggerName);
        //}

        public void JobAdded(IJobDetail jobDetail)
        {
            this.Log().Info("新增job：" + jobDetail.Key.Name);
        }

        public void JobDeleted(JobKey jobKey)
        {
             this.Log().Info("删除Job：" + jobKey.Name);
        }

        public void JobPaused(JobKey jobKey)
        {
            this.Log().Info("Job暂停：" + jobKey.Name);
        }

        public void JobResumed(JobKey jobKey)
        {
            this.Log().Info("Job重新启动：" + jobKey.Name);
        }

        public void JobUnscheduled(TriggerKey triggerKey)
        {
            this.Log().Info("卸载Job：" + triggerKey.Name);
        }

        public void JobsPaused(string jobGroup)
        {
            this.Log().Info("Job暂停：" + jobGroup);
        }

        public void JobsResumed(string jobGroup)
        {
            this.Log().Info("Job重新启动：" + jobGroup);
        }

        public void SchedulerInStandbyMode()
        {
             this.Log().Info("通过标准模式调度：" );
        }

        public void SchedulerShuttingdown()
        {
             this.Log().Info("调度关闭：");
        }

        public void SchedulerStarted()
        {
             this.Log().Info("调度开始：");
        }

        public void SchedulingDataCleared()
        {
             this.Log().Info("调度数据创建：" );
        }

        public void TriggerPaused(TriggerKey triggerKey)
        {
            this.Log().Info("触发器暂停：" + triggerKey.Name);
        }

        public void TriggerResumed(TriggerKey triggerKey)
        {
            this.Log().Info("触发器重启：" + triggerKey.Name);
        }

        public void TriggersPaused(string triggerGroup)
        {
            this.Log().Info("触发器暂停：" + triggerGroup);
        }

        public void TriggersResumed(string triggerGroup)
        {
            this.Log().Info("触发器重新启动：" + triggerGroup);
        }


        public void JobScheduled(ITrigger trigger)
        {
            this.Log().Info("Job完成调度：" + trigger.Key.Name);
        }

        public void SchedulerError(string msg, SchedulerException cause)
        {
            this.Log().Info("Job调度错误：" + msg + cause.Message);
        }

        public void SchedulerShutdown()
        {
             this.Log().Info("调度器关闭：" );
        }

        public void TriggerFinalized(ITrigger trigger)
        {
            this.Log().Info("触发器终止：" + trigger.Key.Name);
        }


        public void SchedulerStarting()
        {
            throw new NotImplementedException();
        }
    }
}