'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Missing/Collection',
        'Cells/SeriesTitleCell',
        'Cells/EpisodeNumberCell',
        'Cells/EpisodeTitleCell',
        'Cells/RelativeDateCell',
        'Shared/Grid/Pager',
        'Shared/LoadingView'
    ], function (Marionette, Backgrid, MissingCollection, SeriesTitleCell, EpisodeNumberCell, EpisodeTitleCell, RelativeDateCell, GridPager, LoadingView) {
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
                        name : 'airDateUtc',
                        label: 'Air Date',
                        cell : RelativeDateCell
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
