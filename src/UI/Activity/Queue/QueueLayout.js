'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Activity/Queue/QueueCollection',
        'Cells/SeriesTitleCell',
        'Cells/EpisodeNumberCell',
        'Cells/EpisodeTitleCell',
        'Cells/QualityCell',
        'Activity/Queue/QueueStatusCell',
        'Activity/Queue/QueueActionsCell',
        'Activity/Queue/TimeleftCell',
        'Activity/Queue/ProgressCell',
        'Shared/Grid/Pager'
    ], function (Marionette,
                 Backgrid,
                 QueueCollection,
                 SeriesTitleCell,
                 EpisodeNumberCell,
                 EpisodeTitleCell,
                 QualityCell,
                 QueueStatusCell,
                 QueueActionsCell,
                 TimeleftCell,
                 ProgressCell,
                 GridPager) {
        return Marionette.Layout.extend({
            template: 'Activity/Queue/QueueLayoutTemplate',

            regions: {
                table: '#x-queue',
                pager: '#x-queue-pager'
            },

            columns:
                [
                    {
                        name      : 'status',
                        label     : '',
                        cell      : QueueStatusCell,
                        cellValue : 'this'
                    },
                    {
                        name      : 'series',
                        label     : 'Series',
                        cell      : SeriesTitleCell,
                        sortable  : false
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
                        name      : 'quality',
                        label     : 'Quality',
                        cell      : QualityCell,
                        sortable  : false
                    },
                    {
                        name      : 'timeleft',
                        label     : 'Timeleft',
                        cell      : TimeleftCell,
                        cellValue : 'this'
                    },
                    {
                        name      : 'episode',
                        label     : 'Progress',
                        cell      : ProgressCell,
                        cellValue : 'this'
                    },
                    {
                        name      : 'status',
                        label     : '',
                        cell      : QueueActionsCell,
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
