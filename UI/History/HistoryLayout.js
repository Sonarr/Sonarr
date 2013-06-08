"use strict";
define([
    'app',
    'History/Collection',
    'Series/Index/Table/AirDateCell',
    'Shared/Toolbar/ToolbarLayout',
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
                    name      : 'indexer',
                    label     : '',
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'History/IndexerTemplate' })
                },
                {
                    name      : 'Series.Title',
                    label     : 'Series Title',
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'Missing/SeriesTitleTemplate' })
                },
                {
                    name      : 'episode',
                    label     : 'Episode',
                    sortable  : false,
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'Missing/EpisodeColumnTemplate' })
                },
                {
                    name      : 'Episode.Title',
                    label     : 'Episode Title',
                    sortable  : false,
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'History/EpisodeTitleTemplate' })
                },
                {
                    name      : 'quality',
                    label     : 'Quality',
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'History/QualityTemplate' })
                },
                {
                    name      : 'date',
                    label     : 'Grabbed',
                    cell      : 'airDate'
                },
                {
                    name      : 'edit',
                    label     : '',
                    sortable  : false,
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'History/ControlsColumnTemplate' })
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

                this.pager.show(new Backgrid.NzbDronePaginator({
                    columns: this.columns,
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
