'use strict';
define(
    [
        'underscore',
        'marionette',
        'backgrid',
        'Wanted/Missing/MissingCollection',
        'Cells/SeriesTitleCell',
        'Cells/EpisodeNumberCell',
        'Cells/EpisodeTitleCell',
        'Cells/RelativeDateCell',
        'Cells/EpisodeStatusCell',
        'Shared/Grid/Pager',
        'Shared/Toolbar/ToolbarLayout',
        'Shared/LoadingView',
        'Shared/Messenger',
        'Commands/CommandController',
        'backgrid.selectall'
    ], function (_,
                 Marionette,
                 Backgrid,
                 MissingCollection,
                 SeriesTitleCell,
                 EpisodeNumberCell,
                 EpisodeTitleCell,
                 RelativeDateCell,
                 EpisodeStatusCell,
                 GridPager,
                 ToolbarLayout,
                 LoadingView,
                 Messenger,
                 CommandController) {
        return Marionette.Layout.extend({
            template: 'Wanted/Missing/MissingLayoutTemplate',

            regions: {
                missing: '#x-missing',
                toolbar: '#x-toolbar',
                pager  : '#x-pager'
            },

            ui: {
                searchSelectedButton: '.btn i.icon-search'
            },

            columns:
                [
                    {
                        name      : '',
                        cell      : 'select-row',
                        headerCell: 'select-all',
                        sortable  : false
                    },
                    {
                        name    : 'series',
                        label   : 'Series Title',
                        sortable  : false,
                        cell    : SeriesTitleCell
                    },
                    {
                        name    : 'this',
                        label   : 'Episode',
                        sortable  : false,
                        cell    : EpisodeNumberCell
                    },
                    {
                        name    : 'this',
                        label   : 'Episode Title',
                        sortable  : false,
                        cell    : EpisodeTitleCell,
                    },
                    {
                        name    : 'airDateUtc',
                        label   : 'Air Date',
                        cell    : RelativeDateCell
                    },
                    {
                        name    : 'status',
                        label   : 'Status',
                        cell    : EpisodeStatusCell,
                        sortable: false
                    }
                ],

            initialize: function () {
                this.collection = new MissingCollection();

                this.listenTo(this.collection, 'sync', this._showTable);
            },

            onShow: function () {
                this.missing.show(new LoadingView());
                this._showToolbar();
                this.collection.fetch();
            },

            _showTable: function () {
                this.missingGrid = new Backgrid.Grid({
                    columns   : this.columns,
                    collection: this.collection,
                    className : 'table table-hover'
                });

                this.missing.show(this.missingGrid);

                this.pager.show(new GridPager({
                    columns   : this.columns,
                    collection: this.collection
                }));
            },

            _showToolbar: function () {
                var leftSideButtons = {
                    type      : 'default',
                        storeState: false,
                        items     :
                    [
                        {
                            title: 'Search Selected',
                            icon : 'icon-search',
                            callback: this._searchSelected,
                            ownerContext: this
                        },
                        {
                            title: 'Season Pass',
                            icon : 'icon-bookmark',
                            route: 'seasonpass'
                        }
                    ]
                };
                
                var filterOptions = {
                    type          : 'radio',
                    storeState    : false,
                    menuKey       : 'wanted.filterMode',
                    defaultAction : 'monitored',
                    items         :
                    [
                        {
                            key      : 'monitored',
                            title    : '',
                            tooltip  : 'Monitored Only',
                            icon     : 'icon-nd-monitored',
                            callback : this._setFilter
                        },
                        {
                            key      : 'unmonitored',
                            title    : '',
                            tooltip  : 'Unmonitored Only',
                            icon     : 'icon-nd-unmonitored',
                            callback : this._setFilter
                        }
                    ]                    
                };

                this.toolbar.show(new ToolbarLayout({
                    left   :
                        [
                            leftSideButtons
                        ],
                    right  :
                        [
                            filterOptions
                        ],
                    context: this
                }));

                CommandController.bindToCommand({
                    element: this.$('.x-toolbar-left-1 .btn i.icon-search'),
                    command: {
                        name: 'episodeSearch'
                    }
                });
            },
            
            _setFilter: function(buttonContext) {
                var mode = buttonContext.model.get('key');

                this.collection.state.currentPage = 1;
                var promise = this.collection.setFilterMode(mode);
                
                if (buttonContext)
                    buttonContext.ui.icon.spinForPromise(promise);
            },

            _searchSelected: function () {
                var selected = this.missingGrid.getSelectedModels();

                if (selected.length === 0) {
                    Messenger.show({
                        type: 'error',
                        message: 'No episodes selected'
                    });

                    return;
                }

                var ids = _.pluck(selected, 'id');

                CommandController.Execute('episodeSearch', {
                    name    : 'episodeSearch',
                    episodeIds: ids
                });
            }
        });
    });
