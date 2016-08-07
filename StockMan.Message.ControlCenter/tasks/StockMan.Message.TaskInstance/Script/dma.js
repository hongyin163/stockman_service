{
    dif: [],
    ama: [],
    c1: 10,
    c2: 50,
    m: 6,
    i: 0,
    data: [],
    calculate: function (newData) {
        var me = this;
        
        //data格式，
        //e[0],//日期
        //e[1],//开盘 
        //e[2],//收盘
        //e[3],//最高
        //e[4],//最低   
        //e[5],//成交量
        //e[6],//涨跌额
        //e[7],//涨跌幅

        //dif=10日均线-50日均线
        //ama=10日均线
        //debugger;


        for (var j = 0; j < newData.length; j++) {
            me.data.push(newData[j]);
        }
        var dma = [];
        var price = [];
        for (me.i; me.i < me.data.length; me.i++) {
            
            var a = me.getAvg(me.c1, me.data, me.i) - me.getAvg(me.c2, me.data, me.i);
            me.dif.push(a);

            var y = me.getAvg2(me.m, me.dif, me.i);
            me.ama.push(y);

            dma.push([me.data[me.i][0], me.dif[me.i].toFixed(2), me.ama[me.i].toFixed(2)]);
        }
        return dma;
    },
    cutdownContext: function(count) {
        var me = this;
        if (count == 0 || count == 1) {
            me.data = [];
            me.dif = [];
            me.ama = [];
            me.i = 0;
        } else {
            me.dif.splice(0, me.i - count);
            me.ama.splice(0, me.i - count);
            me.data.splice(0, me.i - count);
            me.i = count;
        }
    },
    getAvg: function (c1, data, i) {
        if (i >= c1) {
            var n = 0;
            for (var j = 0; j < c1; j++) {
                n += data[i - j][2];
            }
            return n / c1;
        } else {
            return data[i][2];
        }
    },
    getAvg2: function (c1, tr, i) {
        if (i >= c1) {
            var n = 0;
            for (var j = 0; j < c1; j++) {
                n += tr[i - j];
            }
            return n / c1;
        } else {
            return tr[i];
        }
    },
    getState:function(last){
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
        var isCorss=false;
        if (takeCount >= 2)
        {
            isCorss = Ext.isCross(
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
        }
        
        var avg = Ext.getSlope(ixList);
        if (isCorss && avg > 0)
            return IndexState.Up;
        if (isCorss && avg <0)
            return IndexState.Down;
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
            var isCorss = Ext.isCross2(
                {
                    X : 0,
                    Y : ixs[0][1]
                },
                {
                    X : (takeCount - 1) * x,
                    Y : ixs[takeCount - 1][1]
                },
                {
                    X : 0,
                    Y : ixs[0][2]
                },
                {
                    X : (takeCount - 1) * x,
                    Y : ixs[takeCount -1 ][2]
                }
            );       

            var avg = Ext.getSlope(ixList);
            if (isCorss && avg > 0)
                return "DMA金叉";
        }
        return '';
    }
}