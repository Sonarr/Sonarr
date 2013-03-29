'use strict';

define(['app', 'Quality/QualityProfileCollection', 'Series/Index/SeriesItemView'], function (app, qualityProfileCollection) {
    NzbDrone.Series.Index.SeriesIndexCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Series.Index.SeriesItemView,
        itemViewContainer       : 'tbody',
        template                : 'Series/Index/SeriesIndexTemplate',
        qualityProfileCollection: qualityProfileCollection,
        //emptyView: NzbDrone.Series.EmptySeriesCollectionView,

        initialize: function () {
            this.collection = new NzbDrone.Series.SeriesCollection();
            //Todo: This caused the onRendered event to be trigger twice, which displays two empty collection messages
            //http://stackoverflow.com/questions/13065176/backbone-marionette-composit-view-onrender-executing-twice
            this.collection.fetch();
            this.qualityProfileCollection.fetch();

            this.itemViewOptions = { qualityProfiles: this.qualityProfileCollection };
        },

        ui: {
            table: '.x-series-table'
        },

        onItemRemoved: function () {
            this.ui.table.trigger('update');
        },

        onCompositeCollectionRendered: function () {
            this.ui.table.trigger('update');

            if (!this.tableSorter && this.collection.length > 0) {
                this.tableSorter = this.ui.table.tablesorter({
                    textExtraction: function (node) {
                        return node.innerHTML;
                    },
                    sortList      : [
                        [1, 0]
                    ],
                    headers       : {
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

                this.applySortIcons();

                this.ui.table.bind("sortEnd", function () {
                    this.applySortIcons();
                });
            }
            else {
                this.ui.table.trigger('update');
            }
        },
        //Todo: Remove this from each view that requires it
        applySortIcons               : function () {
            $(this.ui.table).find('th.tablesorter-header .tablesorter-header-inner i').each(function () {
                $(this).remove();
            });

            $(this.ui.table).find('th.tablesorter-header').each(function () {
                if ($(this).hasClass('tablesorter-headerDesc')) {
                    $(this).children('.tablesorter-header-inner').append('<i class="icon-sort-up pull-right">');
                }

                else if ($(this).hasClass('tablesorter-headerAsc')) {
                    $(this).children('.tablesorter-header-inner').append('<i class="icon-sort-down pull-right">');
                }

                else if (!$(this).hasClass('sorter-false')) {
                    $(this).children('.tablesorter-header-inner').append('<i class="icon-sort pull-right">');
                }
            });
        }
    });
});

NzbDrone.Series.Index.EmptySeriesCollectionView = Backbone.Marionette.CompositeView.extend({
    template: 'Series/Index/EmptySeriesCollectionTemplate',
    tagName : 'tr'
});