'use strict';
define(
    [
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'Mixins/AutoComplete'
    ], function (Marionette, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/DownloadClient/Options/DownloadClientOptionsViewTemplate',

            ui: {
                droneFactory : '.x-path'
            },

            onShow: function () {
                this.ui.droneFactory.autoComplete('/directories');
            }
        });

        AsModelBoundView.call(view);
        AsValidatedView.call(view);

        return view;
    });
