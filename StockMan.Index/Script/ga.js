{
    c1:12,
    c2:26,
    m:9,
    data:[],
    emaC1 : [], 
    emaC2 : [], 
    diffEmaM :[],
    bar:[],
    i:1,
    calculate: function (newData) {
        
        var me = this;
        if (newData.length == 0)
            return [];

        for (var z = 0; z < newData.length; z++) {
            me.data.push(newData[z]);
        }

        var macd = [];

     

        //EMA（12）= 前一日EMA（12）×11/13＋今日收盘价×2/13
        //EMA（26）= 前一日EMA（26）×25/27＋今日收盘价×2/27
        //DIFF=今日EMA（12）- 今日EMA（26）
        //DEA（MACD）= 前一日DEA×8/10＋今日DIF×2/10 
        //BAR=2×(DIFF－DEA)
        var diff = 0, dea = 0, bar = 0;
        if (me.i == 1) {
            macd.push([me.data[0][0], diff, dea, bar,0]);
            me.emaC1.push(me.data[0][2]);
            me.emaC2.push(me.data[0][2]);
            me.diffEmaM.push(0);
            me.bar.push(0);
        }

        for (me.i; me.i < me.data.length; me.i++) {
            var ema1 = me.emaC1[me.i - 1];
            var ema2 = me.emaC2[me.i - 1];
            var ema3 = me.getEma(me.data[me.i][2], ema1, me.c1, me.i);//ema1 * (c1 - 1) / (c1 + 1) + data[i][2] * (2 / (c1 + 1));
            var ema4 = me.getEma(me.data[me.i][2], ema2, me.c2, me.i); //ema2 * (c2 - 1) / (c2 + 1) + data[i][2] * (2 / (c2 + 1));
            me.emaC1.push(ema3);
            me.emaC2.push(ema4);

            diff = me.getEmaAvg(me.emaC1, me.c1, me.i) - me.getEmaAvg(me.emaC2, me.c2, me.i);

            //指数移动平均
            var diffEma1 = me.diffEmaM[me.i - 1];
            var diffEma = me.getEma(diff, diffEma1, me.m, me.i);// diffEma1 * (m - 1) / (m + 1) + diff * (2 / (m + 1));
            me.diffEmaM.push(diffEma);
            dea = me.getEmaAvg(me.diffEmaM, me.m, me.i);//macd[i - 1][1] * (m - 1) / (m + 1) + diff * (2 / (m + 1));

            //简单移动平均
            //diffEmaM.push(diff);
            //dea = me.getAvg2(me.m, diffEmaM, i);

            bar = 2 * (diff - dea);
            me.bar.push(bar);

            var barChange=me.getBarChange();
            //var ga=Ext.getSlope()
            macd.push([me.data[me.i][0], diff.toFixed(2), dea.toFixed(2), bar.toFixed(2), barChange.toFixed(2)]);
        }
        return macd;
    },
    cutdownContext: function (count) {
        var me = this;
        if (count == 0 || count == 1) {
            me.data = [];
            me.emaC1 = [];
            me.emaC2 = [];
            me.diffEmaM = [];
            me.i = 1;
        } else {
            me.data.splice(0, me.i - count);
            me.emaC1.splice(0, me.i - count);
            me.emaC2.splice(0, me.i - count);
            me.diffEmaM.splice(0, me.i - count);
            me.i = count;
        }
    },
    getEma: function (close, ema_pr, c1, i) {
        return ema_pr * (c1 - 1) / (c1 + 1) + close * (2 / (c1 + 1));
    },
    getEmaAvg: function (ema, c1, i) {
        var v = 0;
        if (i >= c1) {
            var n = 0;
            for (j = 0; j < c1; j++) {
                n += ema[i - j];
            }
            v = n / c1;
            return v;
        } else {
            return ema[i];
        }

    },
    getState:function(last){
        var takeCount = last.length >= 4 ? 4 : last.length;        
       
        var ixs=[];       
        for(var i=last.length-takeCount;i<last.length;i++){
            ixs.push(last[i]);
        }

        var s= last.sort(function(a,b){
            return a[4]-b[4];
        });
        var max = s[s.length-1][4];
        var min = s[0][4];
        var x = (max - min) / takeCount;

        var ixList=[];
        for(var j=0;j<ixs.length;j++){
            ixList.push([j*x,ixs[j][4]]);
        }

        var avg = Ext.getSlope(ixList);

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
    getBarChange:function(){
        var me=this;
        var n=me.bar.length;
        if(n<=1)
            return 0;
        return me.bar[n-1]-me.bar[n-2];
    },
    getTag:function(last){
        var takeCount = last.length >= 4 ? 4 : last.length;        
       
        var ixs=[];
       
        for(var i=last.length-takeCount;i<last.length;i++){
            ixs.push(last[i]);
        }

        var s= last.sort(function(a,b){
            return a[4]-b[4];
        });

        var max = s[s.length-1][4];
        var min = s[0][4];
        var x = (max - min) / takeCount;

        var ixList=[];
        for(var j=0;j<ixs.length;j++){
            ixList.push([j*x,ixs[j][4]]);
        }

        //判断是否有交叉，有交叉，给警告
        var isCorss=false;
        if (takeCount >= 2)
        {
            var first=  ixs[0][4];
            var second= ixs[ixs.length-1][4];
            if(first<=0&&second>=0)
                isCorss=true;
        }

        var avg = Ext.getSlope(ixList);
        if (isCorss && avg > 0)
            return "加速度金叉";
        return '';
    }
}