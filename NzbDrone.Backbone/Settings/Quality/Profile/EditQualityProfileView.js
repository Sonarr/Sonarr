'use strict';
define(['app', 'Quality/QualityProfileModel'], function () {

    NzbDrone.Settings.Quality.Profile.EditQualityProfileView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Quality/Profile/EditQualityProfileTemplate',
        tagName: 'div',
        className: "modal",

        ui: {
            switch: '.switch'
        },

        events: {
            'click .x-save': 'saveQualityProfile',
            //'click .x-remove': 'removeSeries'
        },

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
            this.ui.switch.bootstrapSwitch();
        },


        saveQualityProfile: function () {
            //Todo: Make sure model is updated with Allowed, Cutoff, Name

            this.model.save();
            this.trigger('saved');
            this.$el.parent().modal('hide');
        },

//        removeSeries: function () {
//            var view = new NzbDrone.Series.Delete.DeleteSeriesView({ model: this.model });
//            NzbDrone.modalRegion.show(view);
//        }
    });

});