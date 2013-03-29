'use strict';

define(['app', 'Settings/Quality/Size/QualitySizeView'], function (app) {
    NzbDrone.Settings.Quality.Size.QualitySizeCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView         : NzbDrone.Settings.Quality.Size.QualitySizeView,
        itemViewContainer: '#quality-sizes-container',
        template         : 'Settings/Quality/Size/QualitySizeCollectionTemplate',

        initialize: function () {
            var test = 1;
        },

        ui: {

        },

        onCompositeCollectionRendered: function () {

        }
    });
});