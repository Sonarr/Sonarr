'use strict';
define([
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
    'backgrid.selectall',
    'Mixins/backbone.signalr.mixin'
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
        template : 'Wanted/Missing/MissingLayoutTemplate',

        regions : {
            missing : '#x-missing',
            toolbar : '#x-toolbar',
            pager   : '#x-pager'
        },

        ui : {
            searchSelectedButton : '.btn i.icon-search'
        },

        columns : [
            {
                name      : '',
                cell      : 'select-row',
                headerCell: 'select-all',
                sortable  : false
            },
            {
                name      : 'series',
                label     : 'Series Title',
                cell      : SeriesTitleCell,
                sortValue : 'series.sortTitle'
            },
            {
                name      : 'this',
                label     : 'Episode',
                cell      : EpisodeNumberCell,
                sortable  : false
            },
            {
                name      : 'this',
                label     : 'Episode Title',
                cell      : EpisodeTitleCell,
                sortable  : false
            },
            {
                name      : 'airDateUtc',
                label     : 'Air Date',
                cell      : RelativeDateCell
            },
            {
                name      : 'status',
                label     : 'Status',
                cell      : EpisodeStatusCell,
                sortable  : false
            }
        ],

        initialize : function () {
            this.collection = new MissingCollection().bindSignalR({ updateOnly: true });

            this.listenTo(this.collection, 'sync', this._showTable);
        },

        onShow : function () {
            this.missing.show(new LoadingView());
            this._showToolbar();
            this.collection.fetch();
        },

        _showTable : function () {
            this.missingGrid = new Backgrid.Grid({
                columns    : this.columns,
                collection : this.collection,
                className  : 'table table-hover'
            });

            this.missing.show(this.missingGrid);

            this.pager.show(new GridPager({
                columns    : this.columns,
                collection : this.collection
            }));
        },

        _showToolbar : function () {
            var leftSideButtons = {
                type       : 'default',
                storeState : false,
                collapse   : true,
                items      : [
                    {
                        title        : 'Search Selected',
                        icon         : 'icon-search',
                        callback     : this._searchSelected,
                        ownerContext : this,
                        className    : 'x-search-selected'
                    },
                    {
                        title        : 'Search All Missing',
                        icon         : 'icon-search',
                        callback     : this._searchMissing,
                        ownerContext : this,
                        className    : 'x-search-missing'
                    },
                    {
                        title : 'Season Pass',
                        icon  : 'icon-bookmark',
                        route : 'seasonpass'
                    },
                    {
                        title      : 'Rescan Drone Factory Folder',
                        icon       : 'icon-refresh',
                        command    : 'downloadedepisodesscan',
                        properties : {
                            sendUpdates : true
                        }
                    }
                ]
            };

            var filterOptions = {
                type          : 'radio',
                storeState    : false,
                menuKey       : 'wanted.filterMode',
                defaultAction : 'monitored',
                items         : [
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
                left    : [
                    leftSideButtons
                ],
                right   : [
                    filterOptions
                ],
                context : this
            }));

            CommandController.bindToCommand({
                element : this.$('.x-search-selected'),
                command : {
                    name : 'episodeSearch'
                }
            });

            CommandController.bindToCommand({
                element : this.$('.x-search-missing'),
                command : {
                    name : 'missingEpisodeSearch'
                }
            });
        },

        _setFilter : function (buttonContext) {
            var mode = buttonContext.model.get('key');

            this.collection.state.currentPage = 1;
            var promise = this.collection.setFilterMode(mode);

            if (buttonContext) {
                buttonContext.ui.icon.spinForPromise(promise);
            }
        },

        _searchSelected : function () {
            var selected = this.missingGrid.getSelectedModels();

            if (selected.length === 0) {
                Messenger.show({
                    type    : 'error',
                    message : 'No episodes selected'
                });

                return;
            }

            var ids = _.pluck(selected, 'id');

            CommandController.Execute('episodeSearch', {
                name       : 'episodeSearch',
                episodeIds : ids
            });
        },

        _searchMissing : function () {
            if (window.confirm('Are you sure you want to search for {0} missing episodes? '.format(this.collection.state.totalRecords) + 'One API request to each indexer will be used for each episode. ' + 'This cannot be stopped once started.')) {
                CommandController.Execute('missingEpisodeSearch', {
                    name : 'missingEpisodeSearch'
                });
            }
        }
    });
});
