"use strict";
define([
    'app',
    'Release/Collection',
    'Shared/SpinnerView',
    'Shared/Toolbar/ToolbarLayout',
    'Shared/Cells/EpisodeNumberCell',
    'Shared/Cells/FileSizeCell',
    'Shared/Cells/ApprovalStatusCell'
],
    function () {
        NzbDrone.Release.Layout = Backbone.Marionette.Layout.extend({
            template: 'Release/LayoutTemplate',

            regions: {
                grid   : '#x-grid',
                toolbar: '#x-toolbar'
            },

            columns: [
                {
                    name    : 'indexer',
                    label   : 'Indexer',
                    sortable: true,
                    cell    : Backgrid.StringCell
                },
                {
                    name    : 'size',
                    label   : 'Size',
                    sortable: true,
                    cell    : NzbDrone.Shared.Cells.FileSizeCell
                },
                {
                    name    : 'title',
                    label   : 'Title',
                    sortable: true,
                    cell    : Backgrid.StringCell
                },
                {
                    name    : 'episodeNumbers',
                    season  : 'seasonNumber',
                    airDate : 'airDate',
                    episodes: 'episodeNumbers',
                    label   : 'season',
                    cell    : NzbDrone.Shared.Cells.EpisodeNumberCell
                },
                {
                    name : 'rejections',
                    label: 'decision',
                    cell : NzbDrone.Shared.Cells.ApprovalStatusCell
                }
            ],

            showTable: function () {
                if (!this.isClosed) {
                    this.grid.show(new Backgrid.Grid(
                        {
                            row       : Backgrid.Row,
                            columns   : this.columns,
                            collection: this.collection,
                            className : 'table table-hover'
                        }));
                }
            },

            initialize: function () {
                this.collection = new NzbDrone.Release.Collection();
                this.fetchPromise = this.collection.fetch();
            },

            onShow: function () {

                var self = this;

                this.grid.show(new NzbDrone.Shared.SpinnerView());

                this.fetchPromise.done(function () {
                    self.showTable();
                });
                //this.toolbar.show(new NzbDrone.Shared.Toolbar.ToolbarLayout({right: [ viewButtons], context: this}));
            }

        });
    });
