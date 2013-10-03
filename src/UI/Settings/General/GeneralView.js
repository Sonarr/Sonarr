'use strict';
define(
    [
        'marionette',
        'Mixins/AsModelBoundView'
    ], function (Marionette, AsModelBoundView) {
        var view = Marionette.ItemView.extend({
            template: 'Settings/General/GeneralTemplate',

            events: {
                'change .x-auth': '_setAuthOptionsVisibility',
                'change .x-ssl': '_setSslOptionsVisibility'
            },

            ui: {
                authToggle : '.x-auth',
                authOptions: '.x-auth-options',
                sslToggle : '.x-ssl',
                sslOptions: '.x-ssl-options'
            },

            onRender: function(){
                if(!this.ui.authToggle.prop('checked')){
                    this.ui.authOptions.hide();
                }

                if(!this.ui.sslToggle.prop('checked')){
                    this.ui.sslOptions.hide();
                }
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
            }
        });

        return AsModelBoundView.call(view);
    });

