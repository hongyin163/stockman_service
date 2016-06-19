using System;
using System.Collections.Generic;
using System.Linq;
using StockMan.EntityModel;

namespace StockMan.Index.Tech
{
    public class Dma : IndexBase, IIndex
    {
        public IList<IndexData> Calculate(IList<PriceInfo> data, System.Collections.Specialized.NameValueCollection parameter)
        {
            int c1 = int.Parse(parameter["c1"] + "");
            int c2 = int.Parse(parameter["c2"] + "");
            int m = int.Parse(parameter["m"] + "");

            IList<double> diff = new List<double>(), ama = new List<double>();
            diff.Add(0);
            ama.Add(0);
            var dma = new List<IndexData> { new IndexData(data[0].date) { 0, 0 } };

            for (int i = 1; i < data.Count; i++)
            {
                var a = this.getAvg(c1, data, i) - this.getAvg(c2, data, i);
                diff.Add(a);

                var y = this.getAvg2(m, diff, i);
                ama.Add(y);

                dma.Add(new IndexData(data[i].date) { diff[i], ama[i] });
            }
            return dma;
        }

        public IList<IndexData> Calculate(IList<IndexData> last, IList<PriceInfo> priceList, System.Collections.Specialized.NameValueCollection parameter)
        {
            if (last.Count >= priceList.Count)
                return new List<IndexData>();
            int c1 = int.Parse(parameter["c1"] + "");
            int c2 = int.Parse(parameter["c2"] + "");
            int m = int.Parse(parameter["m"] + "");

            IList<double> diff = new List<double>(), ama = new List<double>();
            var dma = new List<IndexData>();

            List<PriceInfo> data = null;
            int takeCount = 0;
            if (last.Count <= 0)
            {
                //diff.Add(0);
                //ama.Add(0);
                //data = priceList.OrderBy(p => p.date).ToList();
                //takeCount = 1;
                //dma.Add(new IndexData(priceList[0].date) { 0, 0 });
                //data.AddRange(priceList);
                return Calculate(priceList, parameter);
            }
            else
            {

                DateTime maxTime = last.Max(p => p.date);
                data = priceList
                    .Where(p => p.date > maxTime)
                    .ToList();

                if (data.Count == 0)
                    return new List<IndexData>();

                takeCount = last.Count < c2 ? data.Count : c2;

                //返回小于最大日期最近n条
                IList<PriceInfo> oldData = priceList
                    .Where(p => p.date <= maxTime)
                    .OrderByDescending(p => p.date)
                    .Take(takeCount).ToList();

                data.AddRange(oldData);

                //时间顺序
                data = data.OrderBy(p => p.date).ToList();

                IList<IndexData> ixs = last
                   .OrderByDescending(p => p.date)
                   .Take(takeCount)
                   .OrderBy(p => p.date).ToList();
                foreach (IndexData ix in ixs)
                {
                    diff.Add(ix[0]);
                    ama.Add(ix[1]);
                }

            }


            for (int i = takeCount; i < data.Count; i++)
            {
                var a = this.getAvg(c1, data, i) - this.getAvg(c2, data, i);
                diff.Add(a);

                var y = this.getAvg2(m, diff, i);
                ama.Add(y);

                dma.Add(new IndexData(data[i].date) { diff[i], ama[i] });
            }
            return dma;
        }

        public IndexState GetState(IList<IndexData> last)
        {
            int takeCount = last.Count >= 4 ? 4 : last.Count;
            IList<IndexData> ixs = last.OrderByDescending(p => p.date)
                .Take(takeCount)
                .OrderBy(p => p.date).ToList();
            var max = last.Max(p => p[1]);
            var min = last.Min(p => p[1]);
            var x = (max - min) / takeCount;
            double[][] ixList = ixs.Select((ix, i) => new double[] { i * x, ix[1] }).ToArray();

            //判断是否有交叉，有交叉，给警告
            if (takeCount >= 2)
            {
                var isCorss = base.IsCross(new Point()
                {
                    X = (takeCount - 1) * x,
                    Y = ixs[takeCount - 1][0]
                },
                new Point()
                {
                    X = (takeCount - 2) * x,
                    Y = ixs[takeCount - 2][0]
                },
                new Point()
                {
                    X = (takeCount - 1) * x,
                    Y = ixs[takeCount - 1][1]
                },
                new Point()
                {
                    X = (takeCount - 2) * x,
                    Y = ixs[takeCount - 2][1]
                });
                if (isCorss)
                    return IndexState.Warn;
            }

            var avg = base.GetSlope(ixList);

            if (avg > 0)
            {
                return IndexState.Up;
            }
            else if (avg < 0)
            {
                return IndexState.Down;
            }
            else
            {
                return IndexState.Warn;
            }
        }

        private double getAvg(int c1, IList<PriceInfo> data, int i)
        {
            double v = 0;
            if (i >= c1)
            {
                double n = 0;
                for (int j = 0; j < c1; j++)
                {
                    n += (double)(data[i - j].price ?? 0);
                }
                v = n / (double)c1;
                return v;
            }
            else
            {
                return (double)data[i].price;
            }
        }
        private double getAvg2(int c1, IList<double> tr, int i)
        {
            if (i >= c1)
            {
                double n = 0;
                for (int j = 0; j < c1; j++)
                {
                    n += tr[i - j];
                }
                double v = n / c1;
                return v;
            }
            return tr[i];
        }
    }
}