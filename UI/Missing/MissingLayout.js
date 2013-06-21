"use strict";
define(
    [
        'app',
        'Missing/Row',
        'Missing/Collection',
        'Cells/AirDateCell',
        'Series/Index/Table/SeriesStatusCell',
        'Shared/Toolbar/ToolbarLayout',
        'Cells/SeriesTitleCell',
        'Cells/EpisodeNumberCell',
        'Cells/EpisodeTitleCell',
        'Cells/AirDateCell',
        'Shared/Grid/Pager',
        'Shared/LoadingView'
    ], function (App, MissingRow) {
        NzbDrone.Missing.MissingLayout = Backbone.Marionette.Layout.extend({
            template: 'Missing/MissingLayoutTemplate',

            regions: {
                missing: '#x-missing',
                toolbar: '#x-toolbar',
                pager  : '#x-pager'
            },

            columns:
                [
                    {
                        name    : 'series',
                        label   : 'Series Title',
                        sortable: false,
                        cell    : NzbDrone.Cells.SeriesTitleCell
                    },
                    {
                        name    : 'this',
                        label   : 'Episode',
                        sortable: false,
                        cell    : NzbDrone.Cells.EpisodeNumberCell
                    },
                    {
                        name    : 'this',
                        label   : 'Episode Title',
                        sortable: false,
                        cell    : NzbDrone.Cells.EpisodeTitleCell
                    },
                    {
                        name : 'airDate',
                        label: 'Air Date',
                        cell : NzbDrone.Cells.AirDateCell
                    }
                ],

            _showTable: function () {
                this.missing.show(new Backgrid.Grid({
                        row       : MissingRow,
                        columns   : this.columns,
                        collection: this.missingCollection,
                        className : 'table table-hover'
                    }));

                this.pager.show(new NzbDrone.Shared.Grid.Pager({
                    columns   : this.columns,
                    collection: this.missingCollection
                }));
            },

            onShow: function () {
                var self = this;

                this.missing.show(new NzbDrone.Shared.LoadingView());

                this.missingCollection = new NzbDrone.Missing.Collection();
                this.missingCollection.fetch().done(function () {
                        self._showTable();
                    });
            }
        });
    });
