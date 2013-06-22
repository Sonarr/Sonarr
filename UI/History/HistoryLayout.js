'use strict';
define([
    'app',
    'History/Collection',
    'History/EventTypeCell',
    'Cells/RelativeDateCell',
    'Cells/TemplatedCell',
    'Cells/SeriesTitleCell',
    'Cells/EpisodeNumberCell',
    'Cells/EpisodeTitleCell',
    'Cells/QualityCell',
    'Shared/Toolbar/ToolbarLayout',
    'Shared/Grid/Pager',
    'Shared/Grid/HeaderCell',
    'Shared/LoadingView'
],
    function () {
        NzbDrone.History.HistoryLayout = Backbone.Marionette.Layout.extend({
            template: 'History/HistoryLayoutTemplate',

            regions: {
                history: '#x-history',
                toolbar: '#x-toolbar',
                pager  : '#x-pager'
            },

            columns: [
                {
                    name: 'eventType',
                    label:'',
                    cell : NzbDrone.History.EventTypeCell
                },
                {
                    name    : 'series',
                    label   : 'Series',
                    cell    : NzbDrone.Cells.SeriesTitleCell
                },
                {
                    name    : 'episode',
                    label   : 'Episode',
                    sortable: false,
                    cell    : NzbDrone.Cells.EpisodeNumberCell
                },
                {
                    name    : 'episode',
                    label   : 'Episode Title',
                    sortable: false,
                    cell    : NzbDrone.Cells.EpisodeTitleCell
                },
                {
                    name : 'quality',
                    label: 'Quality',
                    cell    : NzbDrone.Cells.QualityCell
                },
                {
                    name : 'date',
                    label: 'Date',
                    cell : NzbDrone.Cells.RelativeDateCell
                }
            ],

            _showTable: function () {

                this.history.show(new Backgrid.Grid(
                    {
                        row       : NzbDrone.History.Row,
                        columns   : this.columns,
                        collection: this.historyCollection,
                        className : 'table table-hover'
                    }));

                this.pager.show(new NzbDrone.Shared.Grid.Pager({
                    columns   : this.columns,
                    collection: this.historyCollection
                }));
            },

            onShow: function () {
                var self = this;

                this.history.show(new NzbDrone.Shared.LoadingView());

                this.historyCollection = new NzbDrone.History.Collection();
                this.historyCollection.fetch()
                    .done(function () {
                        self._showTable();
                    });

                //this.toolbar.show(new NzbDrone.Shared.Toolbar.ToolbarLayout({right: [ viewButtons], context: this}));
            }

        })
        ;
    })
;
