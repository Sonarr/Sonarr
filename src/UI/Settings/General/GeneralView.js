'use strict';
define(
    [
        'vent',
        'marionette',
        'Commands/CommandController',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'Mixins/CopyToClipboard'
    ], function (vent, Marionette, CommandController, AsModelBoundView, AsValidatedView) {
        var view = Marionette.ItemView.extend({
            template: 'Settings/General/GeneralViewTemplate',

            events: {
                'change .x-auth'         : '_setAuthOptionsVisibility',
                'change .x-ssl'          : '_setSslOptionsVisibility',
                'click .x-reset-api-key' : '_resetApiKey'
            },

            ui: {
                authToggle  : '.x-auth',
                authOptions : '.x-auth-options',
                sslToggle   : '.x-ssl',
                sslOptions  : '.x-ssl-options',
                resetApiKey : '.x-reset-api-key',
                copyApiKey  : '.x-copy-api-key',
                apiKeyInput : '.x-api-key'
            },

            initialize: function () {
                vent.on(vent.Events.CommandComplete, this._commandComplete, this);
            },

            onRender: function(){
                if(!this.ui.authToggle.prop('checked')){
                    this.ui.authOptions.hide();
                }

                if(!this.ui.sslToggle.prop('checked')){
                    this.ui.sslOptions.hide();
                }

                CommandController.bindToCommand({
                    element: this.ui.resetApiKey,
                    command: {
                        name: 'resetApiKey'
                    }
                });
            },

            onShow: function () {
                this.ui.copyApiKey.copyToClipboard(this.ui.apiKeyInput);
            },

            _setAuthOptionsVisibility: function () {

                var showAuthOptions = this.ui.authToggle.prop('checked');

                if (showAuthOptions) {
                    this.ui.authOptions.slideDown();
                }

                else {
                    this.ui.authOptions.slideUp();
                }
            },

            _setSslOptionsVisibility: function () {

                var showSslOptions = this.ui.sslToggle.prop('checked');

                if (showSslOptions) {
                    this.ui.sslOptions.slideDown();
                }

                else {
                    this.ui.sslOptions.slideUp();
                }
            },

            _resetApiKey: function () {
                if (window.confirm("Reset API Key?")) {
                    CommandController.Execute('resetApiKey', {
                        name : 'resetApiKey'
                    });
                }
            },

            _commandComplete: function (options) {
                if (options.command.get('name') === 'resetapikey') {
                    this.model.fetch();
                }
            }
        });

        AsModelBoundView.call(view);
        AsValidatedView.call(view);

        return view;
    });

