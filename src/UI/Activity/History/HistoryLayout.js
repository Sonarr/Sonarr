'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Activity/History/HistoryCollection',
        'Cells/EventTypeCell',
        'Cells/SeriesTitleCell',
        'Cells/EpisodeNumberCell',
        'Cells/EpisodeTitleCell',
        'Activity/History/HistoryQualityCell',
        'Cells/RelativeDateCell',
        'Activity/History/HistoryDetailsCell',
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
                 HistoryQualityCell,
                 RelativeDateCell,
                 HistoryDetailsCell,
                 GridPager,
                 ToolbarLayout,
                 LoadingView) {
        return Marionette.Layout.extend({
            template: 'Activity/History/HistoryLayoutTemplate',

            regions: {
                history: '#x-history',
                toolbar: '#x-history-toolbar',
                pager  : '#x-history-pager'
            },

            columns:
                [
                    {
                        name      : 'eventType',
                        label     : '',
                        cell      : EventTypeCell,
                        cellValue : 'this'
                    },
                    {
                        name      : 'series',
                        label     : 'Series',
                        cell      : SeriesTitleCell
                    },
                    {
                        name      : 'episode',
                        label     : 'Episode',
                        cell      : EpisodeNumberCell,
                        sortable  : false
                    },
                    {
                        name      : 'episode',
                        label     : 'Episode Title',
                        cell      : EpisodeTitleCell,
                        sortable  : false
                    },
                    {
                        name      : 'this',
                        label     : 'Quality',
                        cell      : HistoryQualityCell,
                        sortable  : false
                    },
                    {
                        name      : 'date',
                        label     : 'Date',
                        cell      : RelativeDateCell
                    },
                    {
                        name      : 'this',
                        label     : '',
                        cell      : HistoryDetailsCell,
                        sortable  : false
                    }
                ],

            initialize: function () {
                this.collection = new HistoryCollection({ tableName: 'history' });
                this.listenTo(this.collection, 'sync', this._showTable);
            },

            onShow: function () {
                this.history.show(new LoadingView());
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
                            },
                            {
                                key      : 'deleted',
                                title    : '',
                                tooltip  : 'Deleted',
                                icon     : 'icon-nd-deleted',
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

                if (buttonContext) {
                    buttonContext.ui.icon.spinForPromise(promise);
                }
            }
        });
    });
