'use strict';

define(['app', 'Settings/Quality/QualityProfileView'], function (app) {
    NzbDrone.Settings.Quality.QualityProfileCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Settings.Quality.QualityProfileView,
        itemViewContainer: '#quality-profiles-container',
        template: 'Settings/Quality/QualityProfileCollectionTemplate',

        initialize: function (options) {

        },

        ui:{

        },

        onCompositeCollectionRendered: function()
        {

        }
    });
});