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
            'click .x-edit'  : 'editSeries',
            'click .x-remove': 'removeSeries'
        },

        editSeries: function () {
            var view = new NzbDrone.Settings.Quality.Profile.EditQualityProfileView({ model: this.model});

            NzbDrone.vent.trigger(NzbDrone.Events.OpenModalDialog, {
                view: view
            });
        },

        removeSeries: function () {
            var view = new NzbDrone.Series.Delete.DeleteSeriesView({ model: this.model });
            NzbDrone.vent.trigger(NzbDrone.Events.OpenModalDialog, {
                view: view
            });
        }
    });
});
