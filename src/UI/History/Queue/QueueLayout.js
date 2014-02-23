'use strict';
define(
    [
        'marionette',
        'backgrid',
        'History/Queue/QueueCollection',
        'Cells/SeriesTitleCell',
        'Cells/EpisodeNumberCell',
        'Cells/EpisodeTitleCell',
        'Cells/QualityCell',
        'History/Queue/QueueStatusCell',
        'History/Queue/TimeleftCell',
        'Shared/Grid/Pager'
    ], function (Marionette,
                 Backgrid,
                 QueueCollection,
                 SeriesTitleCell,
                 EpisodeNumberCell,
                 EpisodeTitleCell,
                 QualityCell,
                 QueueStatusCell,
                 TimeleftCell,
                 GridPager) {
        return Marionette.Layout.extend({
            template: 'History/Queue/QueueLayoutTemplate',

            regions: {
                table: '#x-queue',
                pager: '#x-queue-pager'
            },

            columns:
                [
                    {
                        name : 'status',
                        label: '',
                        cell : QueueStatusCell,
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
                        name    : 'quality',
                        label   : 'Quality',
                        cell    : QualityCell,
                        sortable: false
                    },
                    {
                        name      : 'timeleft',
                        label     : 'Timeleft',
                        cell      : TimeleftCell,
                        cellValue : 'this'
                    }
                ],

            initialize: function () {
                this.listenTo(QueueCollection, 'sync', this._showTable);
            },

            onShow: function () {
                this._showTable();
            },

            _showTable: function () {
                this.table.show(new Backgrid.Grid({
                    columns   : this.columns,
                    collection: QueueCollection,
                    className : 'table table-hover'
                }));

                this.pager.show(new GridPager({
                    columns   : this.columns,
                    collection: QueueCollection
                }));
            }
        });
    });
