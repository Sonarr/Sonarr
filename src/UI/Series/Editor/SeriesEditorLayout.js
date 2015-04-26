var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var EmptyView = require('../Index/EmptyView');
var SeriesCollection = require('../SeriesCollection');
var SeriesTitleCell = require('../../Cells/SeriesTitleCell');
var ProfileCell = require('../../Cells/ProfileCell');
var SeriesStatusCell = require('../../Cells/SeriesStatusCell');
var SeasonFolderCell = require('../../Cells/SeasonFolderCell');
var SelectAllCell = require('../../Cells/SelectAllCell');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
var FooterView = require('./SeriesEditorFooterView');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'Series/Editor/SeriesEditorLayoutTemplate',

    regions : {
        seriesRegion : '#x-series-editor',
        toolbar      : '#x-toolbar'
    },

    ui : {
        monitored     : '.x-monitored',
        profiles      : '.x-profiles',
        rootFolder    : '.x-root-folder',
        selectedCount : '.x-selected-count'
    },

    events : {
        'click .x-save'         : '_updateAndSave',
        'change .x-root-folder' : '_rootFolderChanged'
    },

    columns : [
        {
            name       : '',
            cell       : SelectAllCell,
            headerCell : 'select-all',
            sortable   : false
        },
        {
            name  : 'statusWeight',
            label : '',
            cell  : SeriesStatusCell
        },
        {
            name      : 'title',
            label     : 'Title',
            cell      : SeriesTitleCell,
            cellValue : 'this'
        },
        {
            name  : 'profileId',
            label : 'Profile',
            cell  : ProfileCell
        },
        {
            name  : 'seasonFolder',
            label : 'Season Folder',
            cell  : SeasonFolderCell
        },
        {
            name  : 'path',
            label : 'Path',
            cell  : 'string'
        }
    ],

    leftSideButtons : {
        type       : 'default',
        storeState : false,
        items      : [
            {
                title : 'Season Pass',
                icon  : 'icon-sonarr-monitored',
                route : 'seasonpass'
            },
            {
                title          : 'Update Library',
                icon           : 'icon-sonarr-refresh',
                command        : 'refreshseries',
                successMessage : 'Library was updated!',
                errorMessage   : 'Library update failed!'
            }
        ]
    },

    initialize : function() {
        this.seriesCollection = SeriesCollection.clone();
        this.seriesCollection.shadowCollection.bindSignalR();
        this.listenTo(this.seriesCollection, 'save', this.render);

        this.filteringOptions = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'serieseditor.filterMode',
            defaultAction : 'all',
            items         : [
                {
                    key      : 'all',
                    title    : '',
                    tooltip  : 'All',
                    icon     : 'icon-sonarr-all',
                    callback : this._setFilter
                },
                {
                    key      : 'monitored',
                    title    : '',
                    tooltip  : 'Monitored Only',
                    icon     : 'icon-sonarr-monitored',
                    callback : this._setFilter
                },
                {
                    key      : 'continuing',
                    title    : '',
                    tooltip  : 'Continuing Only',
                    icon     : 'icon-sonarr-series-continuing',
                    callback : this._setFilter
                },
                {
                    key      : 'ended',
                    title    : '',
                    tooltip  : 'Ended Only',
                    icon     : 'icon-sonarr-series-ended',
                    callback : this._setFilter
                }
            ]
        };
    },

    onRender : function() {
        this._showToolbar();
        this._showTable();
    },

    onClose : function() {
        vent.trigger(vent.Commands.CloseControlPanelCommand);
    },

    _showTable : function() {
        if (this.seriesCollection.shadowCollection.length === 0) {
            this.seriesRegion.show(new EmptyView());
            this.toolbar.close();
            return;
        }

        this.columns[0].sortedCollection = this.seriesCollection;

        this.editorGrid = new Backgrid.Grid({
            collection : this.seriesCollection,
            columns    : this.columns,
            className  : 'table table-hover'
        });

        this.seriesRegion.show(this.editorGrid);
        this._showFooter();
    },

    _showToolbar : function() {
        this.toolbar.show(new ToolbarLayout({
            left    : [
                this.leftSideButtons
            ],
            right   : [
                this.filteringOptions
            ],
            context : this
        }));
    },

    _showFooter : function() {
        vent.trigger(vent.Commands.OpenControlPanelCommand, new FooterView({
            editorGrid : this.editorGrid,
            collection : this.seriesCollection
        }));
    },

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');

        this.seriesCollection.setFilterMode(mode);
    }
});