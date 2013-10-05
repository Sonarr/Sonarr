'use strict';
define(
    [
        'app',
        'marionette',
        'backgrid',
        'Episode/Activity/EpisodeActivityCollection',
        'Cells/EventTypeCell',
        'Cells/QualityCell',
        'Cells/RelativeDateCell',
        'Shared/LoadingView'
    ], function (App, Marionette, Backgrid, EpisodeActivityCollection, EventTypeCell, QualityCell, RelativeDateCell, LoadingView) {

        return Marionette.Layout.extend({
            template: 'Episode/Activity/EpisodeActivityLayoutTemplate',

            regions: {
                activityTable: '.activity-table'
            },

            columns:
                [
                    {
                        name     : 'eventType',
                        label    : '',
                        cell     : EventTypeCell,
                        cellValue: 'this'
                    },
                    {
                        name : 'sourceTitle',
                        label: 'Source Title',
                        cell : 'string'
                    },
                    {
                        name : 'quality',
                        label: 'Quality',
                        cell : QualityCell
                    },
                    {
                        name : 'date',
                        label: 'Date',
                        cell : RelativeDateCell
                    }
                ],

            initialize: function (options) {
                this.model = options.model;
                this.series = options.series;

                this.collection = new EpisodeActivityCollection({ episodeId: this.model.id });
                this.collection.fetch();
                this.listenTo(this.collection, 'sync', this._showTable);
            },

            onRender: function () {
                this.activityTable.show(new LoadingView());
            },

            _showTable: function () {
                this.activityTable.show(new Backgrid.Grid({
                    collection: this.collection,
                    columns   : this.columns,
                    className : 'table table-hover table-condensed'
                }));
            }
        });
    });
