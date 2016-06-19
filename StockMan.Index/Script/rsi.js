{
    c1: 6,
    c2: 12,
    c3: 24,
    i:0,
    data:[],
    calculate: function (newData) {
        var me = this;
        if (newData.length == 0)
            return [];

        for (var z = 0; z < newData.length; z++) {
            me.data.push(newData[z]);
        }
        var rsi = [];

        //6涨跌额 7涨跌幅

        for (me.i; me.i < me.data.length; me.i++) {

            rsi.push([me.data[me.i][0], me.getRsiValue(me.data, me.i, me.c1), me.getRsiValue(me.data, me.i, me.c2), me.getRsiValue(me.data, me.i, me.c3)]);

        }

        return rsi;
    },
    cutdownContext: function (count) {
        var me = this;
        me.data.splice(0, me.i - count);
        me.i = count;
    },
    getRsiValue: function (data, i, c1) {
        if (i >= c1) {
            var up = 0, down = 0;
            for (var j = i; j > i - c1; j--) {
                if (data[j][6] > 0) {
                    up += data[j][6];
                } else {
                    down += (-data[j][6]);
                }
            }
            if(up + down==0){
                return 50;
            }
            var rsi = 100 * up / (up + down);
            return rsi;
        }
        return 50;
    },
    getState:function(last){

        var lastOne =last[last.length-1];
        if (lastOne[3] > 80 || lastOne[3] < 20)
            return IndexState.Warn;

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

        var ixList0=[],ixList1=[],ixList2=[];
        for(var j=0;j<ixs.length;j++){
            ixList0.push([j*x,ixs[j][1]]);
            ixList1.push([j*x,ixs[j][2]]);
            ixList2.push([j*x,ixs[j][3]]);
        }
      

        var p0 = Ext.getSlope(ixList0);
        var p1 = Ext.getSlope(ixList1);
        var p2 = Ext.getSlope(ixList2);

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
}