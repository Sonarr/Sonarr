'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Health/HealthCollection',
        'System/Info/Health/HealthCell',
        'System/Info/Health/HealthOkView'
    ], function (Marionette, Backgrid, HealthCollection, HealthCell, HealthOkView) {
        return Marionette.Layout.extend({
            template: 'System/Info/Health/HealthLayoutTemplate',

            regions: {
                grid: '#x-health-grid'
            },

            columns:
                [
                    {
                        name: 'type',
                        label: '',
                        cell: HealthCell
                    },
                    {
                        name: 'message',
                        label: 'Message',
                        cell: 'string'
                    }
                ],

            initialize: function () {
                this.listenTo(HealthCollection, 'sync', this.render);
                HealthCollection.fetch();
            },

            onRender : function() {
                if (HealthCollection.length === 0) {
                    this.grid.show(new HealthOkView());
                }

                else {
                    this._showTable();
                }
            },

            _showTable: function() {
                this.grid.show(new Backgrid.Grid({
                    row: Backgrid.Row,
                    columns: this.columns,
                    collection: HealthCollection,
                    className:'table table-hover'
                }));
            }
        });
    });
