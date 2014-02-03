'use strict';
define(
    [
        'marionette',
        'backbone',
        'backgrid',
        'History/Table/HistoryTableLayout',
        'History/Blacklist/BlacklistLayout',
        'History/Queue/QueueLayout'
    ], function (Marionette, Backbone, Backgrid, HistoryTableLayout, BlacklistLayout, QueueLayout) {
        return Marionette.Layout.extend({
            template: 'History/HistoryLayoutTemplate',

            regions: {
                history    : '#history',
                blacklist  : '#blacklist',
                queueRegion: '#queue'
            },

            ui: {
                historyTab: '.x-history-tab',
                blacklistTab: '.x-blacklist-tab',
                queueTab  : '.x-queue-tab'
            },

            events: {
                'click .x-history-tab'   : '_showHistory',
                'click .x-blacklist-tab' : '_showBlacklist',
                'click .x-queue-tab'     : '_showQueue'
            },

            initialize: function (options) {
                if (options.action) {
                    this.action = options.action.toLowerCase();
                }
            },

            onShow: function () {
                switch (this.action) {
                    case 'queue':
                        this._showQueue();
                        break;
                    default:
                        this._showHistory();
                }
            },

            _navigate: function (route) {
                Backbone.history.navigate(route);
            },

            _showHistory: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.history.show(new HistoryTableLayout());
                this.ui.historyTab.tab('show');
                this._navigate('/history');
            },

            _showBlacklist: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.blacklist.show(new BlacklistLayout());
                this.ui.blacklistTab.tab('show');
                this._navigate('/history/blacklist');
            },

            _showQueue: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.queueRegion.show(new QueueLayout());
                this.ui.queueTab.tab('show');
                this._navigate('/history/queue');
            }
        });
    });
