var Marionette = require('marionette');
var Backbone = require('backbone');
var Backgrid = require('backgrid');
var HistoryLayout = require('./History/HistoryLayout');
var BlacklistLayout = require('./Blacklist/BlacklistLayout');
var QueueLayout = require('./Queue/QueueLayout');

module.exports = Marionette.Layout.extend({
    template : 'Activity/ActivityLayoutTemplate',

    regions : {
        queueRegion : '#queue',
        history     : '#history',
        blacklist   : '#blacklist'
    },

    ui : {
        queueTab     : '.x-queue-tab',
        historyTab   : '.x-history-tab',
        blacklistTab : '.x-blacklist-tab'
    },

    events : {
        'click .x-queue-tab'     : '_showQueue',
        'click .x-history-tab'   : '_showHistory',
        'click .x-blacklist-tab' : '_showBlacklist'
    },

    initialize : function(options) {
        if (options.action) {
            this.action = options.action.toLowerCase();
        }
    },

    onShow : function() {
        switch (this.action) {
            case 'history':
                this._showHistory();
                break;
            case 'blacklist':
                this._showBlacklist();
                break;
            default:
                this._showQueue();
        }
    },

    _navigate : function(route) {
        Backbone.history.navigate(route, {
            trigger : false,
            replace : true
        });
    },

    _showHistory : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.history.show(new HistoryLayout());
        this.ui.historyTab.tab('show');
        this._navigate('/activity/history');
    },

    _showBlacklist : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.blacklist.show(new BlacklistLayout());
        this.ui.blacklistTab.tab('show');
        this._navigate('/activity/blacklist');
    },

    _showQueue : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.queueRegion.show(new QueueLayout());
        this.ui.queueTab.tab('show');
        this._navigate('/activity/queue');
    }
});