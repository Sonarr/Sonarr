'use strict';

define(
    [
        'AppLayout',
        'marionette',
        'Settings/Quality/Profile/Edit/EditQualityProfileLayout',
        'Mixins/AsModelBoundView',
        'Settings/Quality/Profile/AllowedLabeler',
        'bootstrap'
    ], function (AppLayout, Marionette, EditProfileView, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Quality/Profile/QualityProfileViewTemplate',
            tagName : 'li',

            ui: {
                'progressbar' : '.progress .bar',
                'deleteButton': '.x-delete'
            },

            events: {
                'click'  : '_editProfile'
            },

            initialize: function () {
                this.listenTo(this.model, 'sync', this.render);
            },

            _editProfile: function () {
                var view = new EditProfileView({ model: this.model, profileCollection: this.model.collection });
                AppLayout.modalRegion.show(view);
            }
        });

        return AsModelBoundView.call(view);
    });
