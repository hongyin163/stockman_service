using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using StockMan.EntityModel;

namespace StockMan.Index.Tech
{
    public class Kdj :IndexBase, IIndex
    {

        public IList<IndexData> Calculate(IList<StockMan.EntityModel.PriceInfo> data, NameValueCollection parameter)
        {

            //n日RSV=（Cn－Ln）÷（Hn－Ln）×100
            //K值=2/3×前一日 K值＋1/3×当日RSV
            //D值=2/3×前一日D值＋1/3×当日K值

            int n = int.Parse(parameter["m"] + "");
            double k = 50, d = 50, j = 50;
            double rsv = 50;
            IList<double> ks = new List<double>(), ds = new List<double>();
            ks.Add(k);
            ds.Add(d);
            var kdj = new List<IndexData>();
            kdj.Add(new IndexData(data[0].date) { k, d, j });

            for (int i = 1; i < data.Count; i++)
            {
                rsv = this.getRsvValue(data, i, n);
                k = (2.0 / 3.0) * ks[i - 1] + (1.0 / 3.0) * rsv;
                d = (2 / 3) * ds[i - 1] + (1.0 / 3.0) * k;
                j = (3.0 * k) - (2.0 * d);

                ks.Add(k);
                ds.Add(d);

                kdj.Add(new IndexData(data[i].date) { Math.Round(k, 2), Math.Round(d, 2), Math.Round(j, 2) });
            }
            return kdj;
        }

        private double getRsvValue(IList<StockMan.EntityModel.PriceInfo> data, int i, int c1)
        {
            //n日RSV=（Cn－Ln）÷（Hn－Ln）×100
            if (i >= c1)
            {
                double Cn = 0, Ln = 0, Hn = 0;
                Cn = (double)data[i].price;
                Ln = Hn = Cn;
                var j = i;
                for (j = i - 1; j >= i - c1; j--)
                {
                    if ((double)data[j].price > Hn)
                    {
                        Hn = (double)data[j].price;
                    }
                    if ((double)data[j].price < Ln)
                    {
                        Ln = (double)data[j].price;
                    }
                }
                double rsv = 100 * ((Cn - Ln) / (Hn - Ln));
                return rsv;
            }
            return 50;
        }


        public IList<IndexData> Calculate(IList<IndexData> last, IList<PriceInfo> priceList, NameValueCollection parameter)
        {
            //Result error=
            if (last.Count >= priceList.Count)
                return new List<IndexData>();

            int n = int.Parse(parameter["m"] + "");
            double k = 50, d = 50, j = 50;
            double rsv = 50;

            IList<double> ks = new List<double>(), ds = new List<double>();
            List<PriceInfo> data = null;
            int takeCount = 0;
            if (last.Count <= 0)
            {
                //ks.Add(k);
                //ds.Add(d);
                //data = priceList.OrderBy(p => p.date).ToList();
                //takeCount = 1;
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

                if (last.Count < n)
                {
                    takeCount = data.Count;
                }
                else
                {
                    takeCount = n;
                }

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
                    ks.Add(ix[0]);
                    ds.Add(ix[1]);
                }

            }

            IList<IndexData> rsult = new List<IndexData>();
            for (int i = takeCount; i < data.Count; i++)
            {
                rsv = this.getRsvValue(data, i, n);
                k = (2.0 / 3.0) * ks[i - 1] + (1.0 / 3.0) * rsv;
                d = (2 / 3) * ds[i - 1] + (1.0 / 3.0) * k;
                j = (3.0 * k) - (2.0 * d);

                ks.Add( Math.Round(k,2));
                ds.Add( Math.Round(d,2));
                rsult.Add(new IndexData(data[i].date) { Math.Round(k, 2), Math.Round(d, 2), Math.Round(j, 2) });
            }
            return rsult;
        }

        public IndexState GetState(IList<IndexData> last)
        {
            int takeCount = last.Count >= 4 ? 4 : last.Count;
            IList<IndexData> ixs = last.OrderByDescending(p => p.date)
                .Take(takeCount)
                .OrderBy(p => p.date).ToList();
            var max = last.Max(p => p[2]);
            var min = last.Min(p => p[2]);
            var x = (max - min) / takeCount;
            double[][] ixList = ixs.Select((ix, i) => new double[] { i * x, ix[2] }).ToArray();

            //判断是否有交叉，有交叉，给警告
            if (takeCount >= 2)
            {
                var isCorss = base.IsCross(new Point()
                {
                    X = (takeCount - 1) * x,
                    Y = ixs[takeCount - 1][1]
                },
                new Point()
                {
                    X = (takeCount - 2) * x,
                    Y = ixs[takeCount - 2][1]
                },
                new Point()
                {
                    X = (takeCount - 1) * x,
                    Y = ixs[takeCount - 1][2]
                },
                new Point()
                {
                    X = (takeCount - 2) * x,
                    Y = ixs[takeCount - 2][2]
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
    }
}