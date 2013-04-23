"use strict";
define(['app', 'Quality/QualityProfileCollection', 'Series/Details/SeasonLayout', 'Series/SeasonCollection'], function () {
    NzbDrone.Series.Details.SeriesDetailsView = Backbone.Marionette.CompositeView.extend({

        itemView         : NzbDrone.Series.Details.SeasonLayout,
        itemViewContainer: '.x-series-seasons',
        template         : 'Series/Details/SeriesDetailsTemplate',

        initialize: function () {
            this.collection = new NzbDrone.Series.SeasonCollection();
            this.collection.fetch({data: { seriesId: this.model.get('id') }});

            //$.backstretch(this.model.get('fanArt'));
        },

        onClose: function(){
            $('.backstretch').remove();
        }
    });
});
