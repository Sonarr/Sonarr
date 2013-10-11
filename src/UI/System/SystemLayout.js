'use strict';
define(
    [
        'backbone',
        'marionette',
        'System/Info/SystemInfoLayout',
        'System/Logs/LogsLayout',
        'System/Update/UpdateLayout'
    ], function (Backbone,
                 Marionette,
                 SystemInfoLayout,
                 LogsLayout,
                 UpdateLayout) {
        return Marionette.Layout.extend({
            template: 'System/SystemLayoutTemplate',

            regions: {
                info   : '#info',
                logs    : '#logs',
                updates: '#updates'
            },

            ui: {
                infoTab  : '.x-info-tab',
                logsTab   : '.x-logs-tab',
                updatesTab: '.x-updates-tab'
            },

            events: {
                'click .x-info-tab'  : '_showInfo',
                'click .x-logs-tab'   : '_showLogs',
                'click .x-updates-tab': '_showUpdates'
            },

            initialize: function (options) {
                if (options.action) {
                    this.action = options.action.toLowerCase();
                }
            },

            onShow: function () {
                switch (this.action) {
                    case 'logs':
                        this._showLogs();
                        break;
                    case 'updates':
                        this._showUpdates();
                        break;
                    default:
                        this._showInfo();
                }
            },

            _navigate:function(route){
                Backbone.history.navigate(route);
            },

            _showInfo: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.info.show(new SystemInfoLayout());
                this.ui.infoTab.tab('show');
                this._navigate('system/info');
            },

            _showLogs: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.logs.show(new LogsLayout());
                this.ui.logsTab.tab('show');
                this._navigate('system/logs');
            },

            _showUpdates: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.updates.show(new UpdateLayout());
                this.ui.updatesTab.tab('show');
                this._navigate('system/updates');
            }
        });
    });

