'use strict';

define(['app', 'Settings/Quality/Size/QualitySizeView'], function () {
    NzbDrone.Settings.Quality.Size.QualitySizeCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView         : NzbDrone.Settings.Quality.Size.QualitySizeView,
        itemViewContainer: '#quality-sizes-container',
        template         : 'Settings/Quality/Size/QualitySizeCollectionTemplate',

        initialize: function () {

        },

        ui: {

        },

        onCompositeCollectionRendered: function () {

        }
    });
});
