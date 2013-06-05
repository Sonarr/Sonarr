"use strict";
define([
    'app',
    'Release/Collection',
    'Shared/SpinnerView',
    'Shared/Toolbar/ToolbarLayout'
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
                    name    : 'age',
                    label   : 'Age',
                    sortable: true,
                    cell    : Backgrid.IntegerCell
                },
                {
                    name    : 'size',
                    label   : 'Size',
                    sortable: true,
                    cell    : Backgrid.IntegerCell
                },
                {
                    name    : 'title',
                    label   : 'Title',
                    sortable: true,
                    cell    : Backgrid.StringCell
                },
                {
                    name : 'seasonNumber',
                    label: 'season',
                    cell : Backgrid.IntegerCell
                },
                {
                    name : 'episodeNumber',
                    label: 'episode',
                    cell : Backgrid.StringCell
                },
                {
                    name : 'approved',
                    label: 'Approved',
                    cell : Backgrid.BooleanCell
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
