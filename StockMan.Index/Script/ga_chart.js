{
    config: {
            name:'macd',
            store: {
            fields: [
                { name: 'date', mapping: 0 },
                //{ name: 'diff', mapping: 1 },
                //{ name: 'dea', mapping: 2 },
                //{ name: 'macd', mapping: 3 },
                { name: 'ga', mapping: 4 }
            ]
            },
        labels: [
            {
                color: '#FF0080',
                text: 'diff',
                margin: 40
            },
            {
                color: '#3D7878',
                text: 'dea',
                margin: 40
            }
        ],
        titleConfig: {
            title: '加速度',
            x:80
        },
        axes: [{
            type: 'numeric',
            position: 'left',
            title: {
                //text: 'Sample Values',
                fontSize: 15
            },
            fields: ['ga'],
            style: {
                floating: true,
                axisLine: false,
                strokeStyle: '#666'
            },
            label: {
                fillStyle: '#666',
                fontWeight: '700'
            },
            grid: true
            ,
            background: {
                fill: {
                    type: 'linear',
                    degrees: 180,
                    stops: [
                        {
                            offset: 0.3,
                            color: 'white'
                        },
                        {
                            offset: 1,
                            color: 'rgba(255,255,255,0)'
                        }
                    ]
                }
            }
        }, {
            type: 'category',
            hidden: true,
            position: 'bottom',
            grid: true,
            //visibleRange: [0.6, 1],
            title: {
                //text: 'Sample Values',
                fontSize: 15
            },
            fields: 'date'
        }],
        series: [
        //    {
        //    type: 'line',
        //    xField: 'date',
        //    yField: 'diff',
        //    title: 'Line',
        //    style: {
        //        smooth: true,
        //        stroke: '#FF0080'
        //    }
        //}, {
        //    type: 'line',
        //    xField: 'date',
        //    yField: 'dea',
        //    title: 'Line',
        //    style: {
        //        smooth: true,
        //        stroke: '#3D7878'
        //    }
        //}, 
        {
            type: 'bar',
            xField: 'date',
            yField: 'ga',
            style: {
                fill: 'blue',
                minBarWidth: 3,
                minGapWidth: 1
            }
            ,
            renderer: function (sprite, config, rendererData, index) {
                var data = rendererData.store.getData().items[index];
                if (!data || !data.data)
                    return {}
                if (data.data.ga >= 0) {
                    var obj = {
                        strokeStyle: '#c00',
                        fill: '#c00',
                    };
                    return obj;
                } else {
                    var obj = {
                        strokeStyle: '#006030',
                        fill: '#006030',
                    };
                    return obj;
                }
            }
        }]
    }
}