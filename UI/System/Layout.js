'use strict';
define(
    [
        'app',
        'marionette',
        'System/StatusModel',
        'System/About/View',
        'Logs/Layout',
        'Shared/Toolbar/ToolbarLayout'
    ], function (App,
                 Marionette,
                 StatusModel,
                 AboutView,
                 LogsLayout,
                 ToolbarLayout) {
        return Marionette.Layout.extend({
            template: 'System/LayoutTemplate',

            regions: {
                toolbar : '#toolbar',
                about   : '#about',
                loading : '#loading'
            },

            leftSideButtons: {
                type      : 'default',
                storeState: false,
                items     :
                    [
                        {
                            title: 'Logs',
                            icon : 'icon-book',
                            route: 'logs'
                        },
                        {
                            title  : 'Check for Update',
                            icon   : 'icon-nd-update',
                            command: 'applicationUpdate'
                        }
                    ]
            },

            initialize: function () {
                this.statusModel = StatusModel;
            },

            onRender: function () {
                this._showToolbar();
                this.about.show(new AboutView({ model: this.statusModel }));
            },

            _showToolbar: function () {
                this.toolbar.show(new ToolbarLayout({
                    left   :
                        [
                            this.leftSideButtons
                        ],
                    context: this
                }));
            }
        });
    });

