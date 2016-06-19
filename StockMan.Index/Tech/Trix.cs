using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StockMan.EntityModel;

namespace StockMan.Index
{
    public class Trix :IndexBase, IIndex
    {
        private double getAvg2(int c1, IList<double> tr, int i)
        {
            double v = 0;
            if (i >= c1)
            {
                double n = 0;
                for (int j = 0; j < c1; j++)
                {
                    n += (double)tr[i - j];
                }
                v = n / c1;
                return v;
            }
            else
            {
                return tr[i];
            }
        }
        private double getEma(double close, double ema_pr, int c1, int i)
        {
            return ema_pr * (c1 - 1) / (c1 + 1) + close * (2.0 / (c1 + 1));
        }
        private double getEmaAvg(IList<double> ema, int c1, int i)
        {
            double v = 0;
            if (i >= c1)
            {
                double n = 0;
                for (int j = 0; j < c1; j++)
                {
                    n += (double)ema[i - j];
                }
                v = n / c1;
                return v;
            }
            else
            {
                return ema[i];
            }

        }

        public IList<IndexData> Calculate(IList<PriceInfo> data, System.Collections.Specialized.NameValueCollection parameter)
        {
            var me = this;
            var tech = new List<IndexData>();
            //1、计算N天的收盘价的指数平均AX
            //AX=（I日）收盘价×2÷（N＋1）＋（I－1）日AX ×（N－1）/（N＋1）
            //2、计算N天的AX的指数平均BX
            //BX=（I日）AX×2÷（N＋1）＋（I－1）日BX ×（N－1）/（N＋1）
            //3、计算N天的BX的指数平均TRIX
            //TRIX=（I日）BX×2÷（N＋1）＋（I－1）日TAIX ×（N－1）/（N＋1）
            //4、计算TRIX的m日移动平均TRMA
            List<double> ax = new List<double>(), bx = new List<double>(), tr = new List<double>(), trix = new List<double>(), trma = new List<double>();
            List<double> axEma = new List<double>(), bxEma = new List<double>(), trEma = new List<double>();
            int c1 = int.Parse(parameter["c1"] + "");
            int c2 = int.Parse(parameter["c2"] + "");
            var x = 0;
            //第一次平均
            ax.Add((double)data[0].price);
            axEma.Add(1);
            for (int i = 1; i < data.Count; i++)
            {
                var a = me.getEma((double)data[i].price, ax[i - 1], c1, i);
                ax.Add(a);
                var y = me.getEmaAvg(ax, c1, i);
                axEma.Add(y);
            }
            //第二次平均
            bx.Add(ax[0]);
            bxEma.Add(axEma[0]);
            for (int i = 1; i < axEma.Count; i++)
            {
                var a = me.getEma(axEma[i], bx[i - 1], c1, i);
                bx.Add(a);

                var y = me.getEmaAvg(bx, c1, i);
                bxEma.Add(y);
            }
            //第三次平均
            tr.Add(bx[0]);
            trEma.Add(bxEma[0]);
            for (int i = 1; i < bxEma.Count; i++)
            {
                var a = me.getEma(bxEma[i], tr[i - 1], c1, i);
                tr.Add(a);

                var y = me.getEmaAvg(tr, c1, i);
                trEma.Add(y);
            }
            //计算trix
            trix.Add(1);
            for (int i = 1; i < trEma.Count; i++)
            {
                double z = (trEma[i] - trEma[i - 1]) / trEma[i - 1] * 100;
                trix.Add(z);
            }
            //计算TRMA
            for (int i = 0; i < trix.Count; i++)
            {
                var y = me.getAvg2(c2, trix, i);
                trma.Add(y);
            }


            for (int i = 0; i < data.Count; i++)
            {
                tech.Add(new IndexData(data[i].date) { trix[i], trma[i], ax[i], axEma[i], bx[i], bxEma[i], tr[i], trEma[i] });
            }

            return tech;
        }

        public IList<IndexData> Calculate(IList<IndexData> last, IList<PriceInfo> priceList, System.Collections.Specialized.NameValueCollection parameter)
        {
            var me = this;
            var tech = new List<IndexData>();
            //1、计算N天的收盘价的指数平均AX
            //AX=（I日）收盘价×2÷（N＋1）＋（I－1）日AX ×（N－1）/（N＋1）
            //2、计算N天的AX的指数平均BX
            //BX=（I日）AX×2÷（N＋1）＋（I－1）日BX ×（N－1）/（N＋1）
            //3、计算N天的BX的指数平均TRIX
            //TRIX=（I日）BX×2÷（N＋1）＋（I－1）日TAIX ×（N－1）/（N＋1）
            //4、计算TRIX的m日移动平均TRMA
            List<double> ax = new List<double>(), bx = new List<double>(), tr = new List<double>(), trix = new List<double>(), trma = new List<double>();
            List<double> axEma = new List<double>(), bxEma = new List<double>(), trEma = new List<double>();
            int c1 = int.Parse(parameter["c1"] + "");
            int c2 = int.Parse(parameter["c2"] + "");
            var x = 0;



            List<PriceInfo> data = null;
            int takeCount = 0;
            if (last.Count <= 0)
            {
                return Calculate(priceList, parameter);
            }
            else
            {

                DateTime maxTime = last.Max(p => p.date);
                data = priceList.Where(p => p.date > maxTime).ToList();

                if (data.Count == 0)
                    return new List<IndexData>();

                takeCount = last.Count < c2 ? data.Count : c2;

                //返回小于最大日期最近n条
                IList<PriceInfo> oldData = priceList.Where(p => p.date <= maxTime).OrderByDescending(p => p.date).Take(takeCount).ToList();

                data.AddRange(oldData);

                //时间顺序
                data = data.OrderBy(p => p.date).ToList();


                IList<IndexData> ixs = last
                 .OrderByDescending(p => p.date)
                 .Take(takeCount)
                 .OrderBy(p => p.date).ToList();
                foreach (IndexData ix in ixs)
                {
                    trix.Add(ix[0]);
                    trma.Add(ix[1]);
                    ax.Add(ix[2]);
                    axEma.Add(ix[3]);
                    bx.Add(ix[4]);
                    bxEma.Add(ix[5]);
                    tr.Add(ix[6]);
                    trEma.Add(ix[7]);
                }
            }


            //第一次平均
            for (int i = takeCount; i < data.Count; i++)
            {
                var a = me.getEma((double)data[i].price, ax[i - 1], c1, i);
                ax.Add(a);
                var y = me.getEmaAvg(ax, c1, i);
                axEma.Add(y);
            }
            //第二次平均
            //bx.Add(ax[0]);
            //bxEma.Add(axEma[0]);
            for (int i = takeCount; i < axEma.Count; i++)
            {
                var a = me.getEma(axEma[i], bx[i - 1], c1, i);
                bx.Add(a);

                var y = me.getEmaAvg(bx, c1, i);
                bxEma.Add(y);
            }
            //第三次平均
            //tr.Add(bx[0]);
            //trEma.Add(bxEma[0]);
            for (int i = takeCount; i < bxEma.Count; i++)
            {
                var a = me.getEma(bxEma[i], tr[i - 1], c1, i);
                tr.Add(a);

                var y = me.getEmaAvg(tr, c1, i);
                trEma.Add(y);
            }
            //计算trix
            //trix.Add(0);
            for (int i = takeCount; i < trEma.Count; i++)
            {
                double z = (trEma[i] - trEma[i - 1]) / trEma[i - 1] * 100;
                trix.Add(z);
            }
            //计算TRMA
            for (int i = takeCount; i < trix.Count; i++)
            {
                var y = me.getAvg2(c2, trix, i);
                trma.Add(y);
            }


            for (int i = takeCount; i < data.Count; i++)
            {
                tech.Add(new IndexData(data[i].date) { trix[i], trma[i], ax[i], axEma[i], bx[i], bxEma[i], tr[i], trEma[i] });
            }

            return tech;
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
            double[][] ixList = ixs.Select((ix, i) => new double[] { i * x, ix[0] }).ToArray();

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
    }
}