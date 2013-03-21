'use strict';

define(['app', 'Missing/MissingItemView'], function (app) {
    NzbDrone.Missing.MissingCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Missing.MissingItemView,
        itemViewContainer: 'tbody',
        template: 'Missing/MissingCollectionTemplate',

        ui:{
            table : '.x-missing-table'
        },

        initialize: function (context, action, query, collection) {
            this.collection = collection;
        },
        onCompositeCollectionRendered: function() {
            this.ui.table.trigger('update');

            if(!this.tableSorter && this.collection.length > 0)
            {
                this.tableSorter = this.ui.table.tablesorter({
                    textExtraction: function (node) {
                        return node.innerHTML;
                    },
                    sortList: [[3,1]],
                    headers: {
                        0: {
                            sorter: 'innerHtml'
                        },
                        1: {
                            sorter: false
                        },
                        2: {
                            sorter: false
                        },
                        3: {
                            sorter: 'date'
                        },
                        4: {
                            sorter: false
                        }
                    }
                });

                //Todo: We should extract these common settings out
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
                        $(this).append('<i class="icon-sort-up pull-right">');
                    });

                    $(this).find('th.headerSortUp').each(function(){
                        $(this).append('<i class="icon-sort-down pull-right">');
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