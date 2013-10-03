'use strict';
define(
    [
        'app',
        'marionette',
        'backgrid',
        'History/Table/HistoryTableLayout',
        'History/Queue/QueueLayout'
    ], function (App,
                 Marionette,
                 Backgrid,
                 HistoryTableLayout,
                 QueueLayout) {
        return Marionette.Layout.extend({
            template: 'History/HistoryLayoutTemplate',

            regions: {
                history: '#history',
                queueRegion  : '#queue'
            },

            ui: {
                historyTab: '.x-history-tab',
                queueTab  : '.x-queue-tab'
            },

            events: {
                'click .x-history-tab'  : '_showHistory',
                'click .x-queue-tab'   : '_showQueue'
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

            _navigate:function(route){
                require(['Router'], function(){
                    App.Router.navigate(route);
                });
            },

            _showHistory: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.history.show(new HistoryTableLayout());
                this.ui.historyTab.tab('show');
                this._navigate('/history');
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
