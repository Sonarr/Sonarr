'use strict';
define(
    [
        'backbone',
        'marionette',
        'System/About/AboutView',
        'System/Logs/LogsLayout',
        'System/Update/UpdateLayout',
        'System/DiskSpace/DiskSpaceLayout'
    ], function (Backbone,
                 Marionette,
                 AboutView,
                 LogsLayout,
                 UpdateLayout,
                 DiskSpaceLayout) {
        return Marionette.Layout.extend({
            template: 'System/SystemLayoutTemplate',

            regions: {
                about   : '#about',
                logs    : '#logs',
                updates: '#updates',
                diskSpace: '#diskspace'
            },

            ui: {
                aboutTab  : '.x-about-tab',
                logsTab   : '.x-logs-tab',
                updatesTab: '.x-updates-tab',
                diskSpaceTab: '.x-diskspace-tab'
            },

            events: {
                'click .x-about-tab'  : '_showAbout',
                'click .x-logs-tab'   : '_showLogs',
                'click .x-updates-tab': '_showUpdates',
                'click .x-diskspace-tab':'_showDiskSpace'
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
                    case 'diskspace':
                        this._showDiskSpace();
                    default:
                        this._showAbout();
                }
            },

            _navigate:function(route){
                Backbone.history.navigate(route);
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
            },
            _showDiskSpace: function (e) {
                if (e) {
                    e.preventDefault();
                }
                this.diskSpace.show(new DiskSpaceLayout());
                this.ui.diskSpaceTab.tab('show');
                this._navigate("system/diskspace");
            }
        });
    });

