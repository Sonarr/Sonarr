var Marionette = require('marionette');
var Backgrid = require('backgrid');
var HistoryCollection = require('../../Activity/History/HistoryCollection');
var EventTypeCell = require('../../Cells/EventTypeCell');
var QualityCell = require('../../Cells/QualityCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var EpisodeActivityActionsCell = require('./EpisodeActivityActionsCell');
var EpisodeActivityDetailsCell = require('./EpisodeActivityDetailsCell');
var NoActivityView = require('./NoActivityView');
var LoadingView = require('../../Shared/LoadingView');

module.exports = Marionette.Layout.extend({
    template   : 'Episode/Activity/EpisodeActivityLayoutTemplate',
    regions    : {activityTable : '.activity-table'},
    columns    : [{
        name      : 'eventType',
        label     : '',
        cell      : EventTypeCell,
        cellValue : 'this'
    }, {
        name  : 'sourceTitle',
        label : 'Source Title',
        cell  : 'string'
    }, {
        name  : 'quality',
        label : 'Quality',
        cell  : QualityCell
    }, {
        name  : 'date',
        label : 'Date',
        cell  : RelativeDateCell
    }, {
        name     : 'this',
        label    : '',
        cell     : EpisodeActivityDetailsCell,
        sortable : false
    }, {
        name     : 'this',
        label    : '',
        cell     : EpisodeActivityActionsCell,
        sortable : false
    }],
    initialize : function(options){
        this.model = options.model;
        this.series = options.series;
        this.collection = new HistoryCollection({
            episodeId : this.model.id,
            tableName : 'episodeActivity'
        });
        this.collection.fetch();
        this.listenTo(this.collection, 'sync', this._showTable);
    },
    onRender   : function(){
        this.activityTable.show(new LoadingView());
    },
    _showTable : function(){
        if(this.collection.any()) {
            this.activityTable.show(new Backgrid.Grid({
                collection : this.collection,
                columns    : this.columns,
                className  : 'table table-hover table-condensed'
            }));
        }
        else {
            this.activityTable.show(new NoActivityView());
        }
    }
});