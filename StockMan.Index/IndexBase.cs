using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace StockMan.Index
{
    public class IndexBase
    {
        /// <summary>
        /// 获取斜率
        /// </summary>
        /// <param name="xy"></param>
        /// <returns></returns>
        protected double GetSlope(double[][] xy)
        {
            IList<double> tan = new List<double>();
            for (int i = 1; i < xy.Length; i++)
            {
                var x1y1 = xy[i];
                var x0y0 = xy[i - 1];

                var x = x1y1[0] - x0y0[0];
                var y = x1y1[1] - x0y0[1];
                tan.Add(y / x);
            }
            double avg = tan.Average();
            return Math.Round(avg, 2);
        }
        /// <summary>
        /// 是否交叉
        /// </summary>
        /// <returns></returns>
        protected bool IsCross(Point x1, Point x2, Point y1, Point y2)
        {
            //y=ax+b
            var a0 = (x1.Y - x2.Y) / (x1.X - x2.X);
            var b0 = x1.Y - a0 * x1.X;

            var a1 = (y1.Y - y2.Y) / (y1.X - y2.X);
            var b1 = y1.Y - a1 * y1.X;

            if (a0.Equals(a1))
                return false;

            var x = (b1 - b0) / (a0 - a1);

            if ((x1.X < x2.X && x > x1.X && x < x2.X)
                || (x1.X > x2.X && x < x1.X && x > x2.X))
            {
                return true;
            }
            else if (x1.X > x2.X && x < x1.X && x > x2.X)
            {
                return false;
            }
            return false;
        }
    }
}