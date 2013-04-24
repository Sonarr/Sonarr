'use strict';

define(['app', 'Quality/QualityProfileCollection', 'Series/Index/Posters/ItemView', 'Config'], function (app, qualityProfileCollection) {

    NzbDrone.Series.Index.Posters.CollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Series.Index.Posters.ItemView,
        itemViewContainer       : '#x-series-posters',
        template                : 'Series/Index/Posters/CollectionTemplate',
        qualityProfileCollection: qualityProfileCollection,

        initialize: function () {
            this.qualityProfileCollection.fetch();

            this.itemViewOptions = { qualityProfiles: this.qualityProfileCollection };
        }
    });
});