'use strict';
define(
    [
        'marionette',
        'backgrid',
        'History/Collection',
        'History/EventTypeCell',
        'Cells/SeriesTitleCell',
        'Cells/EpisodeNumberCell',
        'Cells/EpisodeTitleCell',
        'Cells/QualityCell',
        'Cells/RelativeDateCell',
        'Shared/Grid/Pager',
        'Shared/LoadingView'
    ], function (Marionette, Backgrid, HistoryCollection, EventTypeCell, SeriesTitleCell, EpisodeNumberCell, EpisodeTitleCell, QualityCell, RelativeDateCell, GridPager, LoadingView) {
        return Marionette.Layout.extend({
            template: 'History/HistoryLayoutTemplate',

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
                        cell : SeriesTitleCell
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
                var self = this;

                this.history.show(new LoadingView());

                var collection = new HistoryCollection();
                collection.fetch().done(function () {
                    self._showTable(collection);
                });
            }

        });
    });
