'use strict';

define([
        'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.Quality.QualityView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Quality/QualityTemplate',

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        }
    });
});
