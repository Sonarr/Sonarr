'use strict';
define(
    [
        'underscore',
        'marionette',
        'backgrid',
        'Missing/Collection',
        'Cells/SeriesTitleCell',
        'Cells/EpisodeNumberCell',
        'Cells/EpisodeTitleCell',
        'Cells/RelativeDateCell',
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
                 GridPager,
                 ToolbarLayout,
                 LoadingView,
                 Messenger,
                 CommandController) {
        return Marionette.Layout.extend({
            template: 'Missing/MissingLayoutTemplate',

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
                        sortable: false,
                        cell    : SeriesTitleCell
                    },
                    {
                        name    : 'this',
                        label   : 'Episode',
                        sortable: false,
                        cell    : EpisodeNumberCell
                    },
                    {
                        name    : 'this',
                        label   : 'Episode Title',
                        sortable: false,
                        cell    : EpisodeTitleCell
                    },
                    {
                        name : 'airDateUtc',
                        label: 'Air Date',
                        cell : RelativeDateCell
                    }
                ],

            initialize: function () {
                this.collection = new MissingCollection();

                this.listenTo(this.collection, 'sync', this._showTable);
            },

            onShow: function () {
                this.missing.show(new LoadingView());
                this.collection.fetch();
                this._showToolbar();
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


                this.toolbar.show(new ToolbarLayout({
                    left   :
                        [
                            leftSideButtons
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
