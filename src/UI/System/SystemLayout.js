'use strict';
define(
    [
        'app',
        'marionette',
        'System/About/AboutView',
        'System/Logs/LogsLayout',
        'System/Update/UpdateLayout'
    ], function (App,
                 Marionette,
                 AboutView,
                 LogsLayout,
                 UpdateLayout) {
        return Marionette.Layout.extend({
            template: 'System/SystemLayoutTemplate',

            regions: {
                about   : '#about',
                logs    : '#logs',
                updates : '#updates'
            },

            ui: {
                aboutTab  : '.x-about-tab',
                logsTab   : '.x-logs-tab',
                updatesTab: '.x-updates-tab'
            },

            events: {
                'click .x-about-tab'  : '_showAbout',
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
                        this._showAbout();
                }
            },

            _navigate:function(route){
                require(['Router'], function(){
                    App.Router.navigate(route);
                });
            },

            _showAbout: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.about.show(new AboutView());
                this.ui.aboutTab.tab('show');
                this._navigate('system/about');
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

