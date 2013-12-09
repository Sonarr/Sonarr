'use strict';
define(
    [
        'vent',
        'marionette',
        'backgrid',
        'Series/Index/EmptyView',
        'Series/SeriesCollection',
        'Cells/SeriesTitleCell',
        'Cells/QualityProfileCell',
        'Cells/SeriesStatusCell',
        'Cells/SeasonFolderCell',
        'Shared/Toolbar/ToolbarLayout',
        'Series/Editor/SeriesEditorFooterView'
    ], function (vent,
                 Marionette,
                 Backgrid,
                 EmptyView,
                 SeriesCollection,
                 SeriesTitleCell,
                 QualityProfileCell,
                 SeriesStatusCell,
                 SeasonFolderCell,
                 ToolbarLayout,
                 FooterView) {
        return Marionette.Layout.extend({
            template: 'Series/Editor/SeriesEditorLayoutTemplate',

            regions: {
                seriesRegion: '#x-series-editor',
                toolbar     : '#x-toolbar'
            },

            ui: {
                monitored      : '.x-monitored',
                qualityProfiles: '.x-quality-profiles',
                rootFolder     : '.x-root-folder',
                selectedCount  : '.x-selected-count'
            },

            events: {
                'click .x-save'        : '_updateAndSave',
                'change .x-root-folder': '_rootFolderChanged'
            },

            columns:
                [
                    {
                        name      : '',
                        cell      : 'select-row',
                        headerCell: 'select-all',
                        sortable  : false
                    },
                    {
                        name      : 'statusWeight',
                        label     : '',
                        cell      : SeriesStatusCell
                    },
                    {
                        name     : 'title',
                        label    : 'Title',
                        cell     : SeriesTitleCell,
                        cellValue: 'this'
                    },
                    {
                        name : 'qualityProfileId',
                        label: 'Quality',
                        cell : QualityProfileCell
                    },
                    {
                        name : 'monitored',
                        label: 'Season Folder',
                        cell : SeasonFolderCell
                    },
                    {
                        name : 'path',
                        label: 'Path',
                        cell : 'string'
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
                        },
                        {
                            title         : 'Update Library',
                            icon          : 'icon-refresh',
                            command       : 'refreshseries',
                            successMessage: 'Library was updated!',
                            errorMessage  : 'Library update failed!'
                        }
                    ]
            },

            initialize: function () {
                this.listenTo(SeriesCollection, 'sync', this._showTable);
                this.listenTo(SeriesCollection, 'remove', this._showTable);
            },

            onRender: function () {
                this._showToolbar();
                this._showTable();

                this._fetchCollection();
            },

            onClose: function () {
                vent.trigger(vent.Commands.CloseControlPanelCommand);
            },

            _showTable: function () {
                if (SeriesCollection.length === 0) {
                    this.seriesRegion.show(new EmptyView());
                    this.toolbar.close();
                    return;
                }

                this.editorGrid = new Backgrid.Grid({
                    collection: SeriesCollection,
                    columns   : this.columns,
                    className : 'table table-hover'
                });

                this.seriesRegion.show(this.editorGrid);
                this._showFooter();
            },

            _fetchCollection: function () {
                SeriesCollection.fetch();
            },

            _showToolbar: function () {
                this.toolbar.show(new ToolbarLayout({
                    left   :
                        [
                            this.leftSideButtons
                        ],
                    context: this
                }));
            },

            _showFooter: function () {
                vent.trigger(vent.Commands.OpenControlPanelCommand, new FooterView({ editorGrid: this.editorGrid }));
            }
        });
    });
