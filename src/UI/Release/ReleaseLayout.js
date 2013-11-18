'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Release/ReleaseCollection',
        'Cells/IndexerCell',
        'Cells/EpisodeNumberCell',
        'Cells/FileSizeCell',
        'Cells/QualityCell',
        'Cells/ApprovalStatusCell',
        'Shared/LoadingView'
    ], function (Marionette, Backgrid, ReleaseCollection, IndexerCell, EpisodeNumberCell, FileSizeCell, QualityCell, ApprovalStatusCell, LoadingView) {
        return Marionette.Layout.extend({
            template: 'Release/ReleaseLayoutTemplate',

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

            initialize: function () {
                this.collection = new ReleaseCollection();
                this.listenTo(this.collection, 'sync', this._showTable);
            },

            onRender: function () {
                this.grid.show(new LoadingView());
                this.collection.fetch();
            },

            _showTable: function () {
                if (!this.isClosed) {
                    this.grid.show(new Backgrid.Grid({
                        row       : Backgrid.Row,
                        columns   : this.columns,
                        collection: this.collection,
                        className : 'table table-hover'
                    }));
                }
            }
        });
    });
