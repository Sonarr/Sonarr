'use strict';
define(
    [
        'marionette',
        'Mixins/AsModelBoundView'
    ], function (Marionette, AsModelBoundView) {
        var view = Marionette.ItemView.extend({
                template: 'Settings/General/GeneralTemplate',

                events: {
                    'change .x-auth': '_setAuthOptionsVisibility'
                },

                ui: {
                    authToggle : '.x-auth',
                    authOptions: '.x-auth-options'
                },


                onRender: function(){
                    if(!this.ui.authToggle.prop('checked')){
                        this.ui.authOptions.hide();
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
                }

            });

        return AsModelBoundView.call(view);
    });

