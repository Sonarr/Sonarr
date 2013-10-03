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
        'Shared/Toolbar/ToolbarLayout',
        'Shared/LoadingView'
    ], function (Marionette, Backgrid, MissingCollection, SeriesTitleCell, EpisodeNumberCell, EpisodeTitleCell, RelativeDateCell, GridPager, ToolbarLayout, LoadingView) {
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

            leftSideButtons: {
                type      : 'default',
                storeState: false,
                items     :
                    [
                        {
                            title  : 'Season Pass',
                            icon   : 'icon-bookmark',
                            route  : 'seasonpass'
                        }
                    ]
            },

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


            initialize: function () {
                this.missingCollection = new MissingCollection();

                this.listenTo(this.missingCollection, 'sync', this._showTable);
            },


            onShow: function () {
                this.missing.show(new LoadingView());
                this.missingCollection.fetch();
                this._showToolbar();
            },

            _showToolbar: function () {
                this.toolbar.show(new ToolbarLayout({
                    left   :
                        [
                            this.leftSideButtons
                        ],
                    context: this
                }));
            }
        });
    });
