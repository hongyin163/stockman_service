{
    config: {
        name:'ma',
        labels: [
            {
                color: '#FF0080',
                text: '5',
                margin: 40
            },
            {
                color: '#E1E100',
                text: '10',
                margin: 40
            },
            {
                color: '#3D7878',
                text: '20',
                margin: 40
            },
            {
                color: '#FF8000',
                text: '60',
                margin: 40
            }
        ],
        titleConfig: {
            title: 'MA',
            x: 220
        },
        store: {
                fields: [
                    { name: 'date', mapping: 0 },
                    { name: 'ma1', mapping: 1 },
                    { name: 'ma2', mapping: 2 },
                    { name: 'ma3', mapping: 3 },
                    { name: 'ma4', mapping: 4 }
                ]
        },
        axes: [{
            type: 'numeric',
            position: 'left',
            title: {
                //text: 'Sample Values',
                fontSize: 15
            },
            fields: ['ma1', 'ma2', 'ma3', 'ma4'],
            style: {
                floating: true,
                axisLine: false,
                strokeStyle: '#666',
                estStepSize: 40
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
            {
                type: 'line',
                xField: 'date',
                yField: 'ma1',
                title: 'Line',
                style: {
                    smooth: true,
                    stroke: '#FF0080'
                }
            },
            {
                type: 'line',
                xField: 'date',
                yField: 'ma2',
                title: 'Line',
                style: {
                    smooth: true,
                    stroke: '#E1E100'
                }
            },
            {
                type: 'line',
                xField: 'date',
                yField: 'ma3',
                title: 'Line',
                style: {
                    smooth: true,
                    stroke: '#3D7878'
                }
            },
            {
                type: 'line',
                xField: 'date',
                yField: 'ma4',
                title: 'Line',
                style: {
                    smooth: true,
                    stroke: '#FF8000'
                }
            }
        ]
    },
    initialize: function () {
        var me = this;

        me.callParent();

    }
}