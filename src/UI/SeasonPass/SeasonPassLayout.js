var _ = require('underscore');
var vent = require('vent');
var Backgrid = require('backgrid');
var Marionette = require('marionette');
var EmptyView = require('../Series/Index/EmptyView');
var SeriesCollection = require('../Series/SeriesCollection');
var ToolbarLayout = require('../Shared/Toolbar/ToolbarLayout');
var FooterView = require('./SeasonPassFooterView');
var SelectAllCell = require('../Cells/SelectAllCell');
var SeriesStatusCell = require('../Cells/SeriesStatusCell');
var SeriesTitleCell = require('../Cells/SeriesTitleCell');
var SeriesMonitoredCell = require('../Cells/ToggleCell');
var SeasonsCell = require('./SeasonsCell');
require('../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'SeasonPass/SeasonPassLayoutTemplate',

    regions : {
        toolbar : '#x-toolbar',
        series  : '#x-series'
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
            name       : 'monitored',
            label      : '',
            cell       : SeriesMonitoredCell,
            trueClass  : 'icon-sonarr-monitored',
            falseClass : 'icon-sonarr-unmonitored',
            tooltip    : 'Toggle series monitored status',
            sortable   : false
        },
        {
            name      : 'seasons',
            label     : 'Seasons',
            cell      : SeasonsCell,
            cellValue : 'this'
        }
    ],

    initialize : function() {
        this.seriesCollection = SeriesCollection.clone();
        this.seriesCollection.shadowCollection.bindSignalR();

//        this.listenTo(this.seriesCollection, 'sync', this.render);
        this.listenTo(this.seriesCollection, 'seasonpass:saved', this.render);

        this.filteringOptions = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'seasonpass.filterMode',
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
        this._showTable();
        this._showToolbar();
        this._showFooter();
    },

    onClose : function() {
        vent.trigger(vent.Commands.CloseControlPanelCommand);
    },

    _showToolbar : function() {
        this.toolbar.show(new ToolbarLayout({
            right   : [this.filteringOptions],
            context : this
        }));
    },

    _showTable : function() {
        if (this.seriesCollection.shadowCollection.length === 0) {
            this.series.show(new EmptyView());
            this.toolbar.close();
            return;
        }

        this.columns[0].sortedCollection = this.seriesCollection;

        this.editorGrid = new Backgrid.Grid({
            collection : this.seriesCollection,
            columns    : this.columns,
            className  : 'table table-hover'
        });

        this.series.show(this.editorGrid);
        this._showFooter();
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