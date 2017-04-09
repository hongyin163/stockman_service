using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMan.Message.Task
{
    /// <summary>
    /// 定时器
    /// . 立即执行
    /// w:1-5,t:9:30-11:30|13:00-15:00 15
    /// </summary>
    public class TimeTrigger
    {
        private string timeexp = string.Empty;
        public TimeTrigger(string time)
        {
            timeexp = time;
        }
        public bool IsTrigger()
        {
            return MatchTrigger(this.timeexp);
        }
        private bool MatchTrigger(string time)
        {
            DateTime now = DateTime.Now;
            //执行时间|时间间隔多个时间间隔用|分割开 ，时间间隔单分钟
            //.立即执行
            if (time == ".")
            {
                return true;
            }
            //w:1-5,t:9:30-11:30|13:00-15:00 15
            string[] timeArry = time.Split(',');
            bool pass = true;
            foreach (var item in timeArry)
            {
                int sindex = item.IndexOf(':');
                string type = item.Substring(0, sindex).Trim();
                string val = item.Substring(sindex + 1).Trim();
                if (type == "w")
                {
                    //区间
                    if (MatchWeek(val))
                    {
                        continue;
                    }
                    else
                    {
                        pass = false;
                        break;
                    }

                }
                else if (type == "t")
                {
                    if (MatchTime(val))
                    {
                        continue;
                    }
                    else
                    {
                        pass = false;
                        break;
                    }
                }
            }
            return pass;
        }

        private bool MatchTime(string val)
        {
            //9:30-11:30|13:00-15:00 15
            string[] ts = val.Split(' ');
            string timeSection = ts[0];
            string timeInterval = ts.Length > 1 ? ts[1] : "";
            //时间点 时间区间 
            //多少时间区间
            if (timeSection.IndexOf('|') > 0)
            {
                //多个时间区间
                string[] timeSecArry = timeSection.Split('|');
                foreach (var sec in timeSecArry)
                {
                    if (MatchTimeSection(sec, timeInterval))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (timeSection.IndexOf('-') > 0)
            {
                //单个时间区间
                if (MatchTimeSection(timeSection, timeInterval))
                {
                    return true;
                }
                return false;
            }
            else
            {
                //时间点
                var now = DateTime.Now;
                var startTime = string.Format(now.Year + "-" + now.Month + "-" + now.Day + " {0}", timeSection);
                DateTime startT = DateTime.Parse(startTime);
                if (now.Hour == startT.Hour && now.Minute == startT.Minute)
                {
                    return true;
                }
                return false;
            }

        }

        private bool MatchTimeSection(string timeSection, string timeInterval)
        {
            var start = timeSection.Split('-')[0];
            var end = timeSection.Split('-')[1];
            var now = DateTime.Now;
            var startTime = string.Format(now.Year + "-" + now.Month + "-" + now.Day + " ", start);
            var endTime = string.Format(now.Year + "-" + now.Month + "-" + now.Day + " ", end);
            DateTime startT = DateTime.Parse(startTime);
            DateTime endT = DateTime.Parse(endTime);

            if (now.CompareTo(startT) >= 0 && now.CompareTo(endT) <= 0)
            {
                if (string.IsNullOrEmpty(timeInterval))
                    return true;

                int interval = int.Parse(timeInterval);
                TimeSpan ts = now - startT;
                var minutes = ts.TotalMinutes;
                if (minutes % interval == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool MatchWeek(string val)
        {
            int day = (int)DateTime.Now.DayOfWeek;
            if (day == 0) day = 7;
            DateTime now = DateTime.Now;
            if (val.IndexOf('-') > 0)
            {
                int start = int.Parse(val.Split('-')[0]);
                int end = int.Parse(val.Split('-')[1]);
                if (day >= start && day <= end)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (int.Parse(val) == day)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
