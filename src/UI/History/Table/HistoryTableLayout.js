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
        'Shared/Toolbar/ToolbarLayout',
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
                 ToolbarLayout,
                 LoadingView) {
        return Marionette.Layout.extend({
            template: 'History/Table/HistoryTableLayoutTemplate',

            regions: {
                history: '#x-history',
                toolbar: '#x-history-toolbar',
                pager  : '#x-history-pager'
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

            onShow: function () {
                this.history.show(new LoadingView());
                //this.collection.fetch();
                this._showToolbar();
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

            _showToolbar: function () {
                var filterOptions = {
                    type          : 'radio',
                    storeState    : true,
                    menuKey       : 'history.filterMode',
                    defaultAction : 'all',
                    items         :
                        [
                            {
                                key      : 'all',
                                title    : '',
                                tooltip  : 'All',
                                icon     : 'icon-circle-blank',
                                callback : this._setFilter
                            },
                            {
                                key      : 'grabbed',
                                title    : '',
                                tooltip  : 'Grabbed',
                                icon     : 'icon-nd-downloading',
                                callback : this._setFilter
                            },
                            {
                                key      : 'imported',
                                title    : '',
                                tooltip  : 'Imported',
                                icon     : 'icon-nd-imported',
                                callback : this._setFilter
                            },
                            {
                                key      : 'failed',
                                title    : '',
                                tooltip  : 'Failed',
                                icon     : 'icon-nd-download-failed',
                                callback : this._setFilter
                            }
                        ]
                };

                this.toolbar.show(new ToolbarLayout({
                    right  :
                        [
                            filterOptions
                        ],
                    context: this
                }));
            },

            _setFilter: function(buttonContext) {
                var mode = buttonContext.model.get('key');

                this.collection.state.currentPage = 1;
                var promise = this.collection.setFilterMode(mode);

                if (buttonContext)
                    buttonContext.ui.icon.spinForPromise(promise);
            }
        });
    });
