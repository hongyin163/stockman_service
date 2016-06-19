{
    n: 9,
    i: 1,
    data: [],
    ks: [],
    ds: [],
    calculate: function (newData) {
        var me = this;
        var kdj = [];

        //n日RSV=（Cn－Ln）÷（Hn－Ln）×100
        //K值=2/3×前一日 K值＋1/3×当日RSV
        //D值=2/3×前一日D值＋1/3×当日K值
        for (var z = 0; z < newData.length; z++) {
            me.data.push(newData[z]);
        }
        var k = 50, d = 50, j = 50;
        var rsv = 50;

        if (me.i == 1) {
            me.ks.push(k);
            me.ds.push(d);
            kdj.push([me.data[0][0], k, d, j]);
        }
       
        for (me.i; me.i < me.data.length; me.i++) {
            rsv = me.getRsvValue(me.data, me.i, me.n);
            k = (2 / 3) * (me.ks[me.i - 1]) + (1 / 3) * rsv;
            d = (2 / 3) * (me.ds[me.i - 1]) + (1 / 3) * k;
            j = (3 * k) - (2 * d);

            me.ks.push(k);
            me.ds.push(d);
            kdj.push([me.data[me.i][0], k.toFixed(2), d.toFixed(2), j.toFixed(2)]);
        }

        return kdj;
    },
    cutdownContext: function (count) {
        var me = this;
        if (count == 0 || count == 1) {
            me.data = [];
            me.ks = [];
            me.ds = [];
            me.i = 1;
        } else {
            me.ks.splice(0, me.i - count);
            me.ds.splice(0, me.i - count);
            me.data.splice(0, me.i - count);
            me.i = count;
        }
    },
    getRsvValue: function (data, i, c1) {
        //n日RSV=（Cn－Ln）÷（Hn－Ln）×100
        if (i >= c1) {
            var Cn = 0, Ln = 0, Hn = 0;
            Cn = data[i][2];
            Ln = Hn = Cn;
            var j = i;
            for (j = i - 1; j >= i - c1; j--) {
                if (data[j][2] > Hn) {
                    Hn = data[j][2];
                }
                if (data[j][2] < Ln) {
                    Ln = data[j][2];
                }
            }
            var rsv = 100 * ((Cn - Ln) / (Hn - Ln));
            return rsv;
        }
        return 50;
    },
    getState:function(last){
        var takeCount = last.length >= 3 ? 3 : last.length;        
       
        var ixs=[];
       
        for(var i=last.length-takeCount;i<last.length;i++){
            ixs.push(last[i]);
        }

        var s= last.sort(function(a,b){
            return a[3]-b[3];
        });
        var max = s[s.length-1][3];
        var min = s[0][3];
        var x = (max - min) / takeCount;

        var ixList=[];
        for(var j=0;j<ixs.length;j++){
            ixList.push([j*x,ixs[j][3]]);
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
        return IndexState.Warn;
    },
    getTag:function(last){
        var takeCount = last.length >= 3 ? 3 : last.length;        
       
        var ixs=[];
       
        for(var i=last.length-takeCount;i<last.length;i++){
            ixs.push(last[i]);
        }

        var s= last.sort(function(a,b){
            return a[3]-b[3];
        });
        var max = s[s.length-1][3];
        var min = s[0][3];
        var x = (max - min) / takeCount;

        var ixList=[];
        for(var j=0;j<ixs.length;j++){
            ixList.push([j*x,ixs[j][3]]);
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
            var avg = Ext.getSlope(ixList);
            if (isCorss && avg > 0)        
                return "KDJ金叉";
        }  
        return "";
    }
}