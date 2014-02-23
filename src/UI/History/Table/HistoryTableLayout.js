'use strict';
define(
    [
        'marionette',
        'backgrid',
        'History/HistoryCollection',
        'Cells/EventTypeCell',
        'Cells/SeriesTitleCell',
        'Cells/EpisodeNumberCell',
        'Cells/EpisodeTitleCell',
        'Cells/QualityCell',
        'Cells/RelativeDateCell',
        'History/Table/HistoryDetailsCell',
        'Shared/Grid/Pager',
        'Shared/LoadingView'
    ], function (Marionette,
                 Backgrid,
                 HistoryCollection,
                 EventTypeCell,
                 SeriesTitleCell,
                 EpisodeNumberCell,
                 EpisodeTitleCell,
                 QualityCell,
                 RelativeDateCell,
                 HistoryDetailsCell,
                 GridPager,
                 LoadingView) {
        return Marionette.Layout.extend({
            template: 'History/Table/HistoryTableLayoutTemplate',

            regions: {
                history: '#x-history',
                toolbar: '#x-toolbar',
                pager  : '#x-pager'
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
                        name : 'series',
                        label: 'Series',
                        cell : SeriesTitleCell,
                        sortValue: 'series.title'
                    },
                    {
                        name    : 'episode',
                        label   : 'Episode',
                        sortable: false,
                        cell    : EpisodeNumberCell
                    },
                    {
                        name    : 'episode',
                        label   : 'Episode Title',
                        sortable: false,
                        cell    : EpisodeTitleCell
                    },
                    {
                        name    : 'quality',
                        label   : 'Quality',
                        cell    : QualityCell,
                        sortable: false
                    },
                    {
                        name : 'date',
                        label: 'Date',
                        cell : RelativeDateCell
                    },
                    {
                        name    : 'this',
                        label   : '',
                        cell    : HistoryDetailsCell,
                        sortable: false
                    }
                ],

            initialize: function () {
                this.collection = new HistoryCollection({ tableName: 'history' });
                this.listenTo(this.collection, 'sync', this._showTable);
            },


            _showTable: function (collection) {

                this.history.show(new Backgrid.Grid({
                    columns   : this.columns,
                    collection: collection,
                    className : 'table table-hover'
                }));

                this.pager.show(new GridPager({
                    columns   : this.columns,
                    collection: collection
                }));
            },

            onShow: function () {
                this.history.show(new LoadingView());
                this.collection.fetch();
            }
        });
    });
