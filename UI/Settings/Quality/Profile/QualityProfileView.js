'use strict';

define(
    [
        'app',
        'marionette',
        'Settings/Quality/Profile/EditQualityProfileView',
        'Settings/Quality/Profile/DeleteView',
        'Mixins/AsModelBoundView'

    ], function (App, Marionette, EditProfileView, DeleteProfileView, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Quality/Profile/QualityProfileTemplate',
            tagName : 'tr',

            ui: {
                'progressbar': '.progress .bar'
            },

            events: {
                'click .x-edit'  : 'edit',
                'click .x-remove': 'removeQuality'
            },

            edit: function () {
                var view = new EditProfileView({ model: this.model});
                App.modalRegion.show(view);
            },

            removeQuality: function () {
                var view = new DeleteProfileView({ model: this.model });
                App.modalRegion.show(view);
            }
        });


        return AsModelBoundView.call(view);
    });
