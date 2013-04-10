'use strict';

define(['app', 'Quality/QualityProfileCollection', 'Series/Index/SeriesItemView', 'Config'], function (app, qualityProfileCollection) {
    NzbDrone.Series.Index.SeriesIndexCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Series.Index.SeriesItemView,
        itemViewContainer       : '#x-series',
        template                : 'Series/Index/SeriesIndexTemplate',
        qualityProfileCollection: qualityProfileCollection,
        //emptyView: NzbDrone.Series.EmptySeriesCollectionView,

        getTemplate: function(){
            if (this.viewStyle === 1){
                return 'Series/Index/SeriesIndexGridTemplate';
            }
            else {
                return 'Series/Index/SeriesIndexTemplate';
            }
        },

        ui: {
            table: '.x-series-table'
        },

        events: {
            'click .x-series-change-view': 'changeViewTemplate'
        },

        initialize: function () {
            this.viewStyle = NzbDrone.Config.SeriesViewStyle();

            this.collection = new NzbDrone.Series.SeriesCollection();
            //Todo: This caused the onRendered event to be trigger twice, which displays two empty collection messages
            //http://stackoverflow.com/questions/13065176/backbone-marionette-composit-view-onrender-executing-twice
            this.collection.fetch();
            this.qualityProfileCollection.fetch();

            this.itemViewOptions = { qualityProfiles: this.qualityProfileCollection, viewStyle: this.viewStyle };
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
        },

        changeViewTemplate: function(event) {
            event.preventDefault();
            if ($(event.currentTarget).hasClass('x-series-show-grid')) {
                NzbDrone.Config.SeriesViewStyle(1);
            }

            else {
                NzbDrone.Config.SeriesViewStyle(0);
            }

            this.viewStyle = NzbDrone.Config.SeriesViewStyle();
            this.itemViewOptions.viewStyle = this.viewStyle;
            this.render();
        }
    });
});

NzbDrone.Series.Index.EmptySeriesCollectionView = Backbone.Marionette.CompositeView.extend({
    template: 'Series/Index/EmptySeriesCollectionTemplate',
    tagName : 'tr'
});