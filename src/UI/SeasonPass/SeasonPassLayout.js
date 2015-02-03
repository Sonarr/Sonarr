var Marionette = require('marionette');
var SeriesCollection = require('../Series/SeriesCollection');
var SeasonCollection = require('../Series/SeasonCollection');
var SeriesCollectionView = require('./SeriesCollectionView');
var LoadingView = require('../Shared/LoadingView');
var ToolbarLayout = require('../Shared/Toolbar/ToolbarLayout');
require('../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template     : 'SeasonPass/SeasonPassLayoutTemplate',
    regions      : {
        toolbar : '#x-toolbar',
        series  : '#x-series'
    },
    initialize   : function(){
        this.seriesCollection = SeriesCollection.clone();
        this.seriesCollection.shadowCollection.bindSignalR();
        this.listenTo(this.seriesCollection, 'sync', this.render);
        this.filteringOptions = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'seasonpass.filterMode',
            defaultAction : 'all',
            items         : [{
                key      : 'all',
                title    : '',
                tooltip  : 'All',
                icon     : 'icon-circle-blank',
                callback : this._setFilter
            }, {
                key      : 'monitored',
                title    : '',
                tooltip  : 'Monitored Only',
                icon     : 'icon-nd-monitored',
                callback : this._setFilter
            }, {
                key      : 'continuing',
                title    : '',
                tooltip  : 'Continuing Only',
                icon     : 'icon-play',
                callback : this._setFilter
            }, {
                key      : 'ended',
                title    : '',
                tooltip  : 'Ended Only',
                icon     : 'icon-stop',
                callback : this._setFilter
            }]
        };
    },
    onRender     : function(){
        this.series.show(new SeriesCollectionView({collection : this.seriesCollection}));
        this._showToolbar();
    },
    _showToolbar : function(){
        this.toolbar.show(new ToolbarLayout({
            right   : [this.filteringOptions],
            context : this
        }));
    },
    _setFilter   : function(buttonContext){
        var mode = buttonContext.model.get('key');
        this.seriesCollection.setFilterMode(mode);
    }
});