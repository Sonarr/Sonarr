'use strict';

define(['app', 'Quality/QualityProfileCollection', 'Series/SeriesItemView'], function (app, qualityProfileCollection) {
    NzbDrone.Series.SeriesCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Series.SeriesItemView,
        itemViewContainer: 'tbody',
        template: 'Series/SeriesCollectionTemplate',
        qualityProfileCollection: qualityProfileCollection,

        initialize: function () {
            this.collection = new NzbDrone.Series.SeriesCollection();
            this.collection.fetch();
            this.qualityProfileCollection.fetch();

            this.itemViewOptions = { qualityProfiles: this.qualityProfileCollection };
        },

        ui:{
            table : '.x-series-table'
        },


        onItemRemoved: function()
        {
            this.ui.table.trigger('update');
        },

        onCompositeCollectionRendered: function()
        {
            this.ui.table.trigger('update');

            if(!this.tableSorter && this.collection.length > 0)
            {

                this.tableSorter = this.ui.table.tablesorter();

                this.ui.table.bind("sortEnd",function() {
                    $(this).find('th.header i').each(function(){
                        $(this).remove();
                    });

                    $(this).find('th.headerSortDown').each(function(){
                       $(this).append('<i class="icon-sort-down pull-right">');
                    });

                    $(this).find('th.headerSortUp').each(function(){
                        $(this).append('<i class="icon-sort-up pull-right">');
                    });
                });
                }
            else
            {
                this.ui.table.trigger('update');
            }
        }
    });

});