'use strict';

define(['app', 'Quality/QualityProfileCollection', 'Series/Index/List/ItemView', 'Config'], function (app, qualityProfileCollection) {

    NzbDrone.Series.Index.List.CollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Series.Index.List.ItemView,
        itemViewContainer       : '#x-series-list',
        template                : 'Series/Index/List/CollectionTemplate',
        qualityProfileCollection: qualityProfileCollection,

        initialize: function () {
            this.qualityProfileCollection.fetch();

            this.itemViewOptions = { qualityProfiles: this.qualityProfileCollection };
        }
    });
});