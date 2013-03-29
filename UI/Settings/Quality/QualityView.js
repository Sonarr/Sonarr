'use strict';

define([
    'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.Quality.QualityView = Backbone.Marionette.ItemView.extend({
        template : 'Settings/Quality/QualityTemplate',
        className: 'form-horizontal',

        initialize: function (options) {
            this.qualityProfileCollection = options.qualityProfiles;
            this.model.set({ qualityProfiles: this.qualityProfileCollection });
        },

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        }
    });
});
