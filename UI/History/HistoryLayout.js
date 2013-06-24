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
    function (App,
              HistoryCollection,
              EventTypeCell,
              RelativeDateCell,
              TemplatedCell,
              SeriesTitleCell,
              EpisodeNumberCell,
              EpisodeTitleCell,
              QualityCell,
              ToolbarLayout,
              Pager,
              HeaderCell,
              LoadingView) {
        return Backbone.Marionette.Layout.extend({
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
                    cell : EventTypeCell
                },
                {
                    name    : 'series',
                    label   : 'Series',
                    cell    : SeriesTitleCell
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
                    cell    : QualityCell
                },
                {
                    name : 'date',
                    label: 'Date',
                    cell : RelativeDateCell
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

                this.pager.show(new Pager({
                    columns   : this.columns,
                    collection: this.historyCollection
                }));
            },

            onShow: function () {
                var self = this;

                this.history.show(new LoadingView());

                this.historyCollection = new HistoryCollection();
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
