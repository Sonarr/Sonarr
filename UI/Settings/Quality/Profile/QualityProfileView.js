'use strict';

define([
    'app',
    'Quality/QualityProfileCollection',
    'Settings/Quality/Profile/EditQualityProfileView'

], function () {

    NzbDrone.Settings.Quality.Profile.QualityProfileView = Backbone.Marionette.ItemView.extend({
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
            var view = new NzbDrone.Settings.Quality.Profile.EditQualityProfileView({ model: this.model});
            NzbDrone.modalRegion.show(view);
        },

        removeQuality: function () {
            var view = new NzbDrone.Series.Delete.DeleteSeriesView({ model: this.model });
            NzbDrone.modalRegion.show(view);
        }
    });
});
