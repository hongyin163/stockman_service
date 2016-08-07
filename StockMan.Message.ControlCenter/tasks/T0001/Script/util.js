var IndexState = {
    Up: 1,
    Down: -1,
    Warn: 0
};

var Ext = {
    apply: function (object, config) {

        if (object && config && typeof config === 'object') {
            var i;

            for (i in config) {
                object[i] = config[i];
            }

        }

        return object;
    },
    //是否交叉
    isCross: function (x1, x2, y1, y2) {
        //y=ax+b
        var a0 = (x1.Y - x2.Y) / (x1.X - x2.X);
        var b0 = x1.Y - a0 * x1.X;

        var a1 = (y1.Y - y2.Y) / (y1.X - y2.X);
        var b1 = y1.Y - a1 * y1.X;

        if (a0.toFixed(3) == a1.toFixed(3))
            return false;

        var x = (b1 - b0) / (a0 - a1);

        if ((x1.X < x2.X && x > x1.X && x < x2.X)
            || (x1.X > x2.X && x < x1.X && x > x2.X)) {
            return true;
        }
        //else if (x1.X > x2.X && x < x1.X && x > x2.X) {
        //    return false;
        //}
        return false;
    },
    isCross2: function (x1, x2, y1, y2) {
        //y=ax+b
        var v1 = y1.Y - x1.Y;
        var v2 = y2.Y - x2.Y;
        if (v1 > 0 && v2 < 0)
            return true;
        return false;
    },
    //返回斜率
    //参数格式[[1,2],[2,3],[3,4]]
    getSlope: function (xy) {
        var tan = [];
        for (var i = 1; i < xy.length; i++) {
            var x1y1 = xy[i];
            var x0y0 = xy[i - 1];

            var x = x1y1[0] - x0y0[0];
            var y = x1y1[1] - x0y0[1];
            if (x != 0)
                tan.push(y / x);
        }
        var total = 0;
        for (var j = 0; j < tan.length; j++) {
            total += tan[j];

        }
        var avg = total / tan.length;
        return avg;
    }
};
