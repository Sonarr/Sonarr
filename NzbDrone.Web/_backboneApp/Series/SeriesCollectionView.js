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
                this.tableSorter = this.ui.table.tablesorter({
                    textExtraction: function (node) {
                        return node.innerHTML;
                    },
                    sortList: [[1,0]],
                    headers: {
                        0: {
                            sorter: 'title'
                        },
                        1: {
                            sorter: 'innerHtml'
                        },
                        5: {
                            sorter: 'date'
                        },
                        6: {
                            sorter: false
                        },
                        7: {
                            sorter: false
                        }
                    }
                });

                this.ui.table.find('th.header').each(function(){
                    $(this).append('<i class="icon-sort pull-right">');
                });

                this.ui.table.bind("sortEnd", function() {
                    $(this).find('th.header i').each(function(){
                        $(this).remove();
                    });

                    $(this).find('th.header').each(function () {
                        if (!$(this).hasClass('headerSortDown') && !$(this).hasClass('headerSortUp'))
                            $(this).append('<i class="icon-sort pull-right">');
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