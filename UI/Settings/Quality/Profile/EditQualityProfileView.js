'use strict';
define(['app', 'Quality/QualityProfileModel'], function () {

    NzbDrone.Settings.Quality.Profile.EditQualityProfileView = Backbone.Marionette.ItemView.extend({
        template : 'Settings/Quality/Profile/EditQualityProfileTemplate',
        tagName  : 'div',
        className: "modal",

        events: {
            'click .x-save': 'saveQualityProfile'
        },

        saveQualityProfile: function () {
            //Todo: Make sure model is updated with Allowed, Cutoff, Name

            this.model.save();
            this.trigger('saved');
            this.$el.parent().modal('hide');
        }
    });

});