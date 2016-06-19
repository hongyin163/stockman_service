using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Web;
using StockMan.EntityModel;
namespace StockMan.Index
{

    public class Macd : IndexBase,IIndex
    {

        public IList<IndexData> Calculate(IList<StockMan.EntityModel.PriceInfo> priceList, NameValueCollection parameter)
        {


            if (priceList.Count == 0)
                return new List<IndexData>();

            //EMA（12）= 前一日EMA（12）×11/13＋今日收盘价×2/13
            //EMA（26）= 前一日EMA（26）×25/27＋今日收盘价×2/27
            //DIFF=今日EMA（12）- 今日EMA（26）
            //DEA（MACD）= 前一日DEA×8/10＋今日DIF×2/10 
            //BAR=2×(DIFF－DEA)
            var me = this;
            double diff = 0, dea = 0, bar = 0;
            int c1 = 12, c2 = 26, m = 9;
            c1 = int.Parse(parameter["c1"] + "");
            c2 = int.Parse(parameter["c2"] + "");
            m = int.Parse(parameter["m"] + "");
            IList<IndexData> macd = new List<IndexData>();
            IList<double> emaC1 = new List<double>(), emaC2 = new List<double>(), diffEmaM = new List<double>();

            macd.Add(new IndexData(priceList[0].date) { diff, dea, bar, (double)priceList[0].price, (double)priceList[0].price, 0 });

            emaC1.Add((double)priceList[0].price);
            emaC2.Add((double)priceList[0].price);
            diffEmaM.Add(0);

            for (int i = 1; i < priceList.Count; i++)
            {
                var ema1 = emaC1[i - 1];
                var ema2 = emaC2[i - 1];
                var ema3 = me.getEma((double)priceList[i].price, ema1, c1, i);//ema1 * (c1 - 1) / (c1 + 1) + data[i][2] * (2 / (c1 + 1));
                var ema4 = me.getEma((double)priceList[i].price, ema2, c2, i); //ema2 * (c2 - 1) / (c2 + 1) + data[i][2] * (2 / (c2 + 1));
                emaC1.Add(ema3);
                emaC2.Add(ema4);

                diff = me.getEmaAvg(emaC1, c1, i) - me.getEmaAvg(emaC2, c2, i);

                //指数移动平均
                var diffEma1 = diffEmaM[i - 1];
                double diffEma = me.getEma(diff, diffEma1, m, i);// diffEma1 * (m - 1) / (m + 1) + diff * (2 / (m + 1));
                diffEmaM.Add(diffEma);
                dea = me.getEmaAvg(diffEmaM, m, i);//macd[i - 1][1] * (m - 1) / (m + 1) + diff * (2 / (m + 1));

                //简单移动平均
                //diffEmaM.push(diff);
                //dea = me.getAvg2(m, diffEmaM, i);

                bar = 2 * (diff - dea);
                macd.Add(new IndexData(priceList[i].date) { Math.Round(diff, 2), Math.Round(dea, 2), Math.Round(bar, 2), ema1, ema2, diffEma1 });
            }
            return macd;
        }

        public IList<IndexData> Calculate(IList<IndexData> last, IList<StockMan.EntityModel.PriceInfo> priceList, NameValueCollection parameter)
        {
            if (last.Count >= priceList.Count)
                return new List<IndexData>();

            if (priceList.Count == 0)
                return new List<IndexData>();

            //EMA（12）= 前一日EMA（12）×11/13＋今日收盘价×2/13
            //EMA（26）= 前一日EMA（26）×25/27＋今日收盘价×2/27
            //DIFF=今日EMA（12）- 今日EMA（26）
            //DEA（MACD）= 前一日DEA×8/10＋今日DIF×2/10 
            //BAR=2×(DIFF－DEA)
            var me = this;
            double diff = 0, dea = 0, bar = 0;
            int c1 = 12, c2 = 26, m = 9;
            c1 = int.Parse(parameter["c1"] + "");
            c2 = int.Parse(parameter["c2"] + "");
            m = int.Parse(parameter["m"] + "");
            IList<IndexData> macd = new List<IndexData>();
            IList<double> emaC1 = new List<double>(), emaC2 = new List<double>(), diffEmaM = new List<double>();
            List<PriceInfo> data = new List<PriceInfo>();
            int takeCount = 0;
            if (last.Count <= 0)
            {
                //macd.Add(new IndexData(priceList[0].date) { diff, dea, bar, (double)priceList[0].price, (double)priceList[0].price, 0 });

                //emaC1.Add((double)priceList[0].price);
                //emaC2.Add((double)priceList[0].price);
                //diffEmaM.Add(0);
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


                //m<c1<c2
                if (last.Count < c2)
                {
                    takeCount = last.Count;
                }
                else
                {
                    takeCount = c2;
                }
                //返回小于最大日期最近n条
                IList<PriceInfo> oldData = priceList.Where(p => p.date <= maxTime).OrderByDescending(p => p.date).Take(takeCount).ToList();

                data.AddRange(oldData);

                //时间顺序
                data = data.OrderBy(p => p.date).ToList();

                IList<IndexData> ixs = last.OrderByDescending(p => p.date).Take(takeCount).OrderBy(p => p.date).ToList();
                foreach (IndexData ix in ixs)
                {
                    emaC1.Add(ix[3]);
                    emaC2.Add(ix[4]);
                    diffEmaM.Add(ix[5]);
                }


                //IList<IndexData> ixs1 = last.OrderByDescending(p => p.date).Take(takeCount).OrderBy(p => p.date).ToList();
                //foreach (IndexData ix in ixs1)
                //{
                //    emaC2.Add(ix[4]);
                //}


                //IList<IndexData> ixs2 = last.OrderByDescending(p => p.date).Take(takeCount).OrderBy(p => p.date).ToList();

                //foreach (IndexData ix in ixs)
                //{
                //    diffEmaM.Add(ix[5]);
                //}


            }
            for (int i = takeCount; i < data.Count; i++)
            {
                var ema1 = emaC1[i - 1];
                var ema2 = emaC2[i - 1];
                var ema3 = me.getEma((double)data[i].price, ema1, c1, i);//ema1 * (c1 - 1) / (c1 + 1) + data[i][2] * (2 / (c1 + 1));
                var ema4 = me.getEma((double)data[i].price, ema2, c2, i); //ema2 * (c2 - 1) / (c2 + 1) + data[i][2] * (2 / (c2 + 1));
                emaC1.Add(ema3);
                emaC2.Add(ema4);

                diff = me.getEmaAvg(emaC1, c1, i) - me.getEmaAvg(emaC2, c2, i);

                //指数移动平均
                var diffEma1 = diffEmaM[i - 1];
                double diffEma = me.getEma(diff, diffEma1, m, i);// diffEma1 * (m - 1) / (m + 1) + diff * (2 / (m + 1));
                diffEmaM.Add(diffEma);
                dea = me.getEmaAvg(diffEmaM, m, i);//macd[i - 1][1] * (m - 1) / (m + 1) + diff * (2 / (m + 1));

                //简单移动平均
                //diffEmaM.push(diff);
                //dea = me.getAvg2(m, diffEmaM, i);

                bar = 2 * (diff - dea);
                macd.Add(new IndexData(data[i].date) { Math.Round(diff, 2), Math.Round(dea, 2), Math.Round(bar, 2), ema1, ema2, diffEma1 });
            }
            return macd;
        }

        public IndexState GetState(IList<IndexData> last)
        {
            int takeCount = last.Count > 4 ? 4 : last.Count;
            IList<IndexData> ixs = last.OrderByDescending(p => p.date)
                .Take(takeCount)
                .OrderBy(p => p.date).ToList();
            //0dif 1dea 2bar
            double dif, dea, bar;

            var max = last.Max(p => p[2]);
            var min = last.Min(p => p[2]);
            var x = (max - min) / takeCount;
            double[][] ixList = ixs.Select((ix, i) => new double[] { i * x, ix[2] }).ToArray();


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


            //int upCount = 0, downCount = 0;
            //for (int i = 1; i < tan.Count; i++)
            //{
            //    if (tan[i] > tan[i - 1])
            //    {
            //        upCount++;
            //    }
            //    else
            //    {
            //        downCount++;
            //    }
            //}


            //如果当前为绿柱，考察bar的斜率是否为正

            //如果当前为红柱，考察斜率是否正，过去一段时间红多绿少

            //红多绿少，且斜率为正，状态上涨

            //对交叉点，给警告

            //否则该下跌状态
        }

        public double getEma(double close, double ema_pr, int c1, int i)
        {
            return ema_pr * ((double)c1 - 1) / ((double)c1 + 1) + close * (2 / ((double)c1 + 1));
        }
        public double getEmaAvg(IList<double> ema, int c1, int i)
        {
            double v = 0;
            if (i >= c1)
            {
                double n = 0;
                for (int j = 0; j < c1; j++)
                {
                    n += ema[i - j];
                }
                v = n / (double)c1;
                return v;
            }
            else
            {
                return ema[i];
            }

        }
    }
}