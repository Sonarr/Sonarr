'use strict';

define(['app', 'Settings/Quality/QualitySizeView'], function (app) {
    NzbDrone.Settings.Quality.QualitySizeCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Settings.Quality.QualitySizeView,
        itemViewContainer: '#quality-sizes-container',
        template: 'Settings/Quality/QualitySizeCollectionTemplate',

        initialize: function () {
            var test = 1;
        },

        ui:{

        },

        onCompositeCollectionRendered: function()
        {

        }
    });
});