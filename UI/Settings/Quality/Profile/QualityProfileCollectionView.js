'use strict';

define(['app', 'Settings/Quality/Profile/QualityProfileView'], function (app) {
    NzbDrone.Settings.Quality.Profile.QualityProfileCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Settings.Quality.Profile.QualityProfileView,
        itemViewContainer: 'tbody',
        template: 'Settings/Quality/Profile/QualityProfileCollectionTemplate',

        initialize: function (options) {
        },

        ui:{

        },

        onCompositeCollectionRendered: function()
        {

        }
    });
});