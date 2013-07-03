'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Release/Collection',
        'Cells/IndexerCell',
        'Cells/EpisodeNumberCell',
        'Cells/FileSizeCell',
        'Cells/QualityCell',
        'Cells/ApprovalStatusCell',
        'Shared/SpinnerView'
    ], function (Marionette, Backgrid, ReleaseCollection, IndexerCell, EpisodeNumberCell, FileSizeCell, QualityCell, ApprovalStatusCell, SpinnerView) {
        return Marionette.Layout.extend({
            template: 'Release/LayoutTemplate',

            regions: {
                grid   : '#x-grid',
                toolbar: '#x-toolbar'
            },

            columns:
                [
                    {
                        name    : 'indexer',
                        label   : 'Indexer',
                        sortable: true,
                        cell    : IndexerCell
                    },

                    {
                        name    : 'title',
                        label   : 'Title',
                        sortable: true,
                        cell    : Backgrid.StringCell
                    },
                    {
                        name    : 'episodeNumbers',
                        episodes: 'episodeNumbers',
                        label   : 'season',
                        cell    : EpisodeNumberCell
                    },
                    {
                        name    : 'size',
                        label   : 'Size',
                        sortable: true,
                        cell    : FileSizeCell
                    },
                    {
                        name    : 'quality',
                        label   : 'Quality',
                        sortable: true,
                        cell    : QualityCell
                    },

                    {
                        name : 'rejections',
                        label: '',
                        cell : ApprovalStatusCell
                    }
                ],

            showTable: function () {
                if (!this.isClosed) {
                    this.grid.show(new Backgrid.Grid({
                        row       : Backgrid.Row,
                        columns   : this.columns,
                        collection: this.collection,
                        className : 'table table-hover'
                    }));
                }
            },

            initialize: function () {
                this.collection = new ReleaseCollection();
                this.fetchPromise = this.collection.fetch();
            },

            onShow: function () {

                var self = this;

                this.grid.show(new SpinnerView());

                this.fetchPromise.done(function () {
                    self.showTable();
                });
            }

        });
    });
