'use strict';
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
        'Shared/Grid/Pager',
        'Shared/LoadingView'
    ], function (App,
                 MissingRow,
                 MissingCollection,
                 AirDateCell,
                 SeriesStatusCell,
                 ToolbarLayout,
                 SeriesTitleCell,
                 EpisodeNumberCell,
                 EpisodeTitleCell,
                 Pager,
                 LoadingView) {
        return Backbone.Marionette.Layout.extend({
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
                        cell    : SeriesTitleCell
                    },
                    {
                        name    : 'this',
                        label   : 'Episode',
                        sortable: false,
                        cell    : EpisodeNumberCell
                    },
                    {
                        name    : 'this',
                        label   : 'Episode Title',
                        sortable: false,
                        cell    : EpisodeTitleCell
                    },
                    {
                        name    : 'airDate',
                        label   : 'Air Date',
                        cell    : AirDateCell
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

                this.missing.show(new LoadingView());

                this.missingCollection = new MissingCollection();
                this.missingCollection.fetch().done(function () {
                        self._showTable();
                    });
            }
        });
    });
