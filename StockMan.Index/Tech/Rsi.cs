using System;
using System.Collections.Generic;
using System.Linq;
using StockMan.EntityModel;

namespace StockMan.Index.Tech
{
    public class Rsi : IndexBase, IIndex
    {
        public IList<IndexData> Calculate(IList<PriceInfo> data, System.Collections.Specialized.NameValueCollection parameter)
        {

            var rsi = new List<IndexData>();
            //var c1 = 6, c2 = 12, c3 = 24;

            int c1 = int.Parse(parameter["c1"] + "");
            int c2 = int.Parse(parameter["c2"] + "");
            int c3 = int.Parse(parameter["c3"] + "");
            //6涨跌额 7涨跌幅

            for (int i = 1; i < data.Count; i++)
            {

                rsi.Add(new IndexData(data[i].date)
                {
                    this.getRsiValue(data, i, c1),
                    this.getRsiValue(data, i, c2),
                    this.getRsiValue(data, i, c3)
                });
            }
            return rsi;
        }

        public IList<IndexData> Calculate(IList<IndexData> last, IList<PriceInfo> priceList, System.Collections.Specialized.NameValueCollection parameter)
        {
            var rsi = new List<IndexData>();
            //var c1 = 6, c2 = 12, c3 = 24;

            int c1 = int.Parse(parameter["c1"] + "");
            int c2 = int.Parse(parameter["c2"] + "");
            int c3 = int.Parse(parameter["c3"] + "");

            List<PriceInfo> data = null;
            int takeCount = 0;
            if (last.Count <= 0)
            {
                //data = priceList.OrderBy(p => p.date).ToList();
                //takeCount = 1;
                //data.AddRange(priceList);
                return Calculate(priceList, parameter);
            }
            else
            {

                DateTime maxTime = last.Max(p => p.date);
                data = priceList.Where(p => p.date > maxTime).ToList();

                if (data.Count == 0)
                    return new List<IndexData>();

                takeCount = last.Count < c3 ? data.Count : c3;

                //返回小于最大日期最近n条
                IList<PriceInfo> oldData = priceList.Where(p => p.date <= maxTime).OrderByDescending(p => p.date).Take(takeCount).ToList();

                data.AddRange(oldData);

                //时间顺序
                data = data.OrderBy(p => p.date).ToList();

            }

            for (int i = takeCount; i < data.Count; i++)
            {

                rsi.Add(new IndexData(data[i].date)
                {
                    this.getRsiValue(data, i, c1),
                    this.getRsiValue(data, i, c2),
                    this.getRsiValue(data, i, c3)
                });
            }
            return rsi;


        }

        public IndexState GetState(IList<IndexData> last)
        {
            var lastOne = last.Last();
            if (lastOne[2] > 80 || lastOne[2] < 20)
                return IndexState.Warn;

            int takeCount = last.Count >= 4 ? 4 : last.Count;
            IList<IndexData> ixs = last.OrderByDescending(p => p.date)
                .Take(takeCount)
                .OrderBy(p => p.date).ToList();
            var max = last.Max(p => p[1]);
            var min = last.Min(p => p[1]);
            var x = (max - min) / takeCount;
            double[][] ixList0 = ixs.Select((ix, i) => new double[] { i * x, ix[0] }).ToArray();
            double[][] ixList1 = ixs.Select((ix, i) => new double[] { i * x, ix[1] }).ToArray();
            double[][] ixList2 = ixs.Select((ix, i) => new double[] { i * x, ix[2] }).ToArray();

            var p0 = base.GetSlope(ixList0);
            var p1 = base.GetSlope(ixList1);
            var p2 = base.GetSlope(ixList2);
            if (p0 > 0 && p1 > 0)
            {
                return IndexState.Up;
            }
            else if (p0 < 0 && p1 < 0)
            {
                return IndexState.Down;
            }
            else
            {
                return IndexState.Down;
            }
        }


        private double getRsiValue(IList<PriceInfo> data, int i, int c1)
        {
            if (i >= c1)
            {
                double up = 0, down = 0;
                for (int j = i; j > i - c1; j--)
                {
                    if (data[j].updown > 0)
                    {
                        up += (double)data[j].updown;
                    }
                    else
                    {
                        down += (double)(-data[j].updown);
                    }
                }
                var rsi = 100 * up / (up + down);
                return rsi;
            }
            return 50;
        }
    }
}