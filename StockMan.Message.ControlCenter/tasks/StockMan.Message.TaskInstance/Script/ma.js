{
    c1: 5,
    c2: 10,
    c3: 20,
    c4: 60,
    ma1: [],
    ma2: [],
    ma3: [],
    ma4: [],
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
        var ma = [];
        var price = [];
        for (me.i; me.i < me.data.length; me.i++) {

            var a = me.getAvg(me.c1, me.data, me.i);
            me.ma1.push(a);

            a = me.getAvg(me.c2, me.data, me.i);
            me.ma2.push(a);

            a = me.getAvg(me.c3, me.data, me.i);
            me.ma3.push(a);

            a = me.getAvg(me.c4, me.data, me.i);
            me.ma4.push(a);

            ma.push([me.data[me.i][0], me.ma1[me.i].toFixed(2), me.ma2[me.i].toFixed(2), me.ma3[me.i].toFixed(2), me.ma4[me.i].toFixed(2)]);
        }
        return ma;
    },
    cutdownContext: function (count) {
        var me = this;
        if (count == 0 || count == 1) {
            me.data = [];
            me.ma1 = [];
            me.ma2 = [];
            me.ma3 = [];
            me.ma4 = [];
            me.i = 0;
        } else {
            me.ma1.splice(0, me.i - count);
            me.ma2.splice(0, me.i - count);
            me.ma3.splice(0, me.i - count);
            me.ma4.splice(0, me.i - count);
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

        var ixList = [];
        for (var j = 0; j < ixs.length; j++) {
            ixList.push([j * x, ixs[j][1]]);
        }

        //判断是否有交叉，有交叉，给警告
        var isCorss = false;
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
        if (isCorss && avg < 0)
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
    getTag: function (last) {
       
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

        var ixList = [];
        for (var j = 0; j < ixs.length; j++) {
            ixList.push([j * x, ixs[j][1]]);
        }

        //判断是否有交叉，有交叉，给警告
        if (takeCount >= 2) {
            var avg = Ext.getSlope(ixList);
            if(avg<0)
                return '';
            var v5=ixs[takeCount - 1][1];
            var v10=ixs[takeCount - 1][2];
            var v20=ixs[takeCount - 1][3];
            var v60=ixs[takeCount - 1][4];
            //5日叉10日
            if(v60>v20&&v20>v10&&v10>=v5){
                var v10_5=v10-v5;
                if(v10_5>=0&&v10_5<0.5){
                    return "均线5叉10";
                }
            }

            //5日叉20日
            if(v60>v20&&v20>v10&&v5>v10&&v5<=v20){
                var v20_5=v20-v5;
                if(v20_5>=0&&v20_5<0.5){
                    return "均线5叉20";
                }
            }
            //5日叉60日
            if(v60>v20&&v20>v10&&v5>v20&&v5<=v60){
                var v60_5=v60-v5;
                if(v60_5>=0&&v60_5<1){
                    return "均线5叉60";
                }
            }
            //10日叉60日
            if(v60>v20&&v10>v20&&v10<=v60){
                var v60_10=v60-v10;
                if(v60_10>=0&&v60_10<1){
                    return "均线10叉60";
                }
            }          
        
        }
        return '';
    }
}