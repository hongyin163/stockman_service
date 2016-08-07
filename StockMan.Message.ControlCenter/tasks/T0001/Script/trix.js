{
    c1: 10,
    c2: 20,
    ax: [], bx: [], tr: [],
    axEma: [], bxEma: [], trEma: [],
    trix: [],
    trma: [],
    i: 1,
    data: [],
    calculate: function (newData) {
        var me = this;
        if (newData.length == 0)
            return [];

        for (var z = 0; z < newData.length; z++) {
            me.data.push(newData[z]);
        }
        var tech = [];
        //1、计算N天的收盘价的指数平均AX
        //AX=（I日）收盘价×2÷（N＋1）＋（I－1）日AX ×（N－1）/（N＋1）
        //2、计算N天的AX的指数平均BX
        //BX=（I日）AX×2÷（N＋1）＋（I－1）日BX ×（N－1）/（N＋1）
        //3、计算N天的BX的指数平均TRIX
        //TRIX=（I日）BX×2÷（N＋1）＋（I－1）日TAIX ×（N－1）/（N＋1）
        //4、计算TRIX的m日移动平均TRMA
        //var ax = [], bx = [], tr = [], trix = [], trma = [];
        //var axEma = [], bxEma = [], trEma = [];
        //var trma = [];
        var x = 0;
        //第一次平均
        if (me.i == 1) {
            me.ax.push(me.data[0][2]);
            me.axEma.push(1);
        }
        var j = me.i;
        for (j; j < me.data.length; j++) {
            var a0 = me.getEma(me.data[j][2], me.ax[j - 1], me.c1, j);
            me.ax.push(a0);
            var a1 = me.getEmaAvg(me.ax, me.c1, j);
            me.axEma.push(a1);
            //delete ax;
        }
        //第二次平均
        if (me.i == 1) {
            me.bx.push(me.ax[0]);
            me.bxEma.push(me.axEma[0]);
        }
        j = me.i;
        for (j; j < me.axEma.length; j++) {
            var a2 = me.getEma(me.axEma[j], me.bx[j - 1], me.c1, j);
            me.bx.push(a2);

            var a3 = me.getEmaAvg(me.bx, me.c1, j);
            me.bxEma.push(a3);
        }
        //第三次平均
        if (me.i == 1) {
            me.tr.push(me.bx[0]);
            me.trEma.push(me.bxEma[0]);
        }
        j = me.i;
        for (j; j < me.bxEma.length; j++) {
            var a = me.getEma(me.bxEma[j], me.tr[j - 1], me.c1, j);
            me.tr.push(a);

            var y = me.getEmaAvg(me.tr, me.c1, j);
            me.trEma.push(y);
        }
        //计算trix
        if (me.i == 1) {
            me.trix.push(0);
        }
        j = me.i;
        for (j; j < me.trEma.length; j++) {
            var x1 = (me.trEma[j] - me.trEma[j - 1]) / me.trEma[j - 1] * 100;
            me.trix.push(x1);
        }
        //计算TRMA
        if (me.i == 1) {
            me.trma.push(me.trix[0]);
        }
        j = me.i;
        for (j; j < me.trix.length; j++) {
            var x2 = me.getAvg2(me.c2, me.trix, j);
            me.trma.push(x2);
        }
        if (me.i == 1) {
            tech.push([me.data[0][0], me.trix[0], me.trma[0]]);
        }
        for (me.i; me.i < me.data.length; me.i++) {
            tech.push([me.data[me.i][0], me.trix[me.i], me.trma[me.i]]);
        }

        return tech;
    },
    cutdownContext: function (count) {
        var me = this;
        if (count == 0 || count == 1) {
            me.ax = [];
            me.bx = [];
            me.tr = [];
            me.axEma = [];
            me.bxEma = [];
            me.trEma = [];
            me.trix = [];
            me.trma = [];
            me.data = [];
            me.i = 1;
        } else {
            me.ax.splice(0, me.i - count);
            me.bx.splice(0, me.i - count);
            me.tr.splice(0, me.i - count);
            me.axEma.splice(0, me.i - count);
            me.bxEma.splice(0, me.i - count);
            me.trEma.splice(0, me.i - count);
            me.trix.splice(0, me.i - count);
            me.trma.splice(0, me.i - count);
            me.data.splice(0, me.i - count);
            me.i = count;
        }
    },
    getTrixAvg: function (n, data, i, ax) {
        var me = this;
        var x = data[i][2];
        if (i >= n && i > 0 && (i - 1) < ax.length) {
            x = data[i][2] * 2 / (n + 1) + ax[i - 1] * (n - 1) / (n + 1);
        }
        return x;
    },
    getTrixAvg2: function (n, ax, i, bx) {
        var me = this;
        var x = ax[i];
        if (i >= n && i > 0 && (i - 1) < bx.length) {
            x = ax[i] * 2 / (n + 1) + bx[i - 1] * (n - 1) / (n + 1);
        }
        return x;
    },
    getAvg2: function (c1, tr, i) {
        var v = 0;
        if (i >= c1) {
            var n = 0;
            for (var j = 0; j < c1; j++) {
                n += tr[i - j];
            }
            v = n / c1;
            return v;
        } else {
            return tr[i];
        }
    },
    getEma: function (close, emaPr, c1, i) {
        return emaPr * (c1 - 1) / (c1 + 1) + close * (2 / (c1 + 1));
    },
    getEmaAvg: function (ema, c1, i) {
        var v = 0;
        if (i >= c1) {
            var n = 0;
            for (var j = 0; j < c1; j++) {
                n += ema[i - j];
            }
            v = n / c1;
            return v;
        } else {
            return ema[i];
        }

    },
    getState: function (last) {
        var takeCount = last.length >= 4 ? 4 : last.length;

        var ixs = [];

        for (var i = last.length - takeCount; i < last.length; i++) {
            ixs.push(last[i]);
        }

        var s = last.sort(function (a, b) {
            return a[2] - b[2];
        });


        var max = s[s.length - 1][2];
        var min = s[0][2];
        var x = (max - min) / takeCount;
        //double[][] ixList = ixs.Select((ix, i) => new double[] { i * x, ix[1] }).ToArray();
        var ixList = [];
        for (var j = 0; j < ixs.length; j++) {
            ixList.push([j * x, ixs[j][1]]);
        }

        //判断是否有交叉，有交叉，给警告
        var isCorss=false;
        if (takeCount >= 2) {
            isCorss = Ext.isCross(
                {
                    X: (takeCount - 1) * x,
                    Y: ixs[takeCount - 1][1]
                },
                {
                    X: (takeCount - 2) * x,
                    Y: ixs[takeCount - 2][1]
                },
                {
                    X: (takeCount - 1) * x,
                    Y: ixs[takeCount - 1][2]
                },
                {
                    X: (takeCount - 2) * x,
                    Y: ixs[takeCount - 2][2]
                }
            );
        }

        var avg = Ext.getSlope(ixList);

        if (isCorss && avg > 0)
            return IndexState.Up;
        if (isCorss && avg <0)
            return IndexState.Down;

        if (avg > 0) {
            return IndexState.Up;
        }
        else if (avg < 0) {
            return IndexState.Down;
        }
        else {
            return IndexState.Warn;
        }
    },
    getTag:function(last){
        var takeCount = last.length >= 4 ? 4 : last.length;        
       
        var ixs=[];
       
        for(var i=last.length-takeCount;i<last.length;i++){
            ixs.push(last[i]);
        }

        var s= last.sort(function(a,b){
            return a[2]-b[2];
        });

        var max = s[s.length-1][2];
        var min = s[0][2];
        var x = (max - min) / takeCount;

        var ixList=[];
        for(var j=0;j<ixs.length;j++){
            ixList.push([j*x,ixs[j][1]]);
        }

        //判断是否有交叉，有交叉，给警告
        if (takeCount >= 2)
        {
            var isCorss = Ext.isCross(
                {
                    X : (takeCount - 1) * x,
                    Y : ixs[takeCount - 1][1]
                },
                {
                    X : (takeCount - 2) * x,
                    Y : ixs[takeCount - 2][1]
                },
                {
                    X : (takeCount - 1) * x,
                    Y : ixs[takeCount - 1][2]
                },
                {
                    X : (takeCount - 2) * x,
                    Y : ixs[takeCount - 2][2]
                }
            );       

            var avg = Ext.getSlope(ixList);
            if (isCorss && avg > 0)
                return "TRIX金叉";
        }
        return '';
    }
}