'use strict';

define(
    [
        'app',
        'marionette',
        'Settings/Quality/Profile/EditQualityProfileView',
        'Settings/Quality/Profile/DeleteView',
        'Mixins/AsModelBoundView',
        'Settings/Quality/Profile/AllowedLabeler'

    ], function (App, Marionette, EditProfileView, DeleteProfileView, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Quality/Profile/QualityProfileTemplate',
            tagName : 'li',

            ui: {
                'progressbar': '.progress .bar'
            },

            events: {
                'click .x-edit'  : '_editProfile',
                'click .x-delete': '_deleteProfile'
            },

            initialize: function () {
                this.listenTo(this.model, 'sync', this.render);
            },

            _editProfile: function () {
                var view = new EditProfileView({ model: this.model, profileCollection: this.model.collection });
                App.modalRegion.show(view);
            },

            _deleteProfile: function () {
                var view = new DeleteProfileView({ model: this.model });
                App.modalRegion.show(view);
            }
        });

        return AsModelBoundView.call(view);
    });
