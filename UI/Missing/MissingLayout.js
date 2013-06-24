'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Missing/Collection',
        'Cells/SeriesTitleCell',
        'Cells/EpisodeNumberCell',
        'Cells/EpisodeTitleCell',
        'Cells/AirDateCell',
        'Shared/Grid/Pager',
        'Shared/LoadingView'
    ], function (Marionette, Backgrid, MissingCollection, SeriesTitleCell, EpisodeNumberCell, EpisodeTitleCell, AirDateCell, GridPager, LoadingView) {
        return Marionette.Layout.extend({
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
                        name : 'airDate',
                        label: 'Air Date',
                        cell : AirDateCell
                    }
                ],

            _showTable: function () {
                this.missing.show(new Backgrid.Grid({
                    columns   : this.columns,
                    collection: this.missingCollection,
                    className : 'table table-hover'
                }));

                this.pager.show(new GridPager({
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
