'use strict';

define(['app', 'Missing/MissingItemView'], function (app) {
    NzbDrone.Missing.MissingCollectionView = Backbone.Marionette.CompositeView.extend({
        itemView: NzbDrone.Missing.MissingItemView,
        itemViewContainer: 'tbody',
        template: 'Missing/MissingCollectionTemplate',

        ui:{
            table : '.x-missing-table',
            pager : '.x-missing-table-pager'
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

                this.ui.table.bind('pagerComplete pagerInitialized', function(event, c){
                    c.container.find('.page-number').text(c.page + 1);
                });

                this.ui.table.tablesorterPager({
                    container: this.ui.pager,
                    output: 'Displaying {startRow} to {endRow} of {totalRows} episodes'
                });

                this.applySortIcons();

                this.ui.table.bind("sortEnd", function() {
                    this.applySortIcons();
                });
            }
            else
            {
                this.ui.table.trigger('update');
            }
    	},

        //Todo: Remove this from each view that requires it
        applySortIcons: function() {
            $(this.ui.table).find('th.tablesorter-header .tablesorter-header-inner i').each(function(){
                $(this).remove();
            });

            $(this.ui.table).find('th.tablesorter-header').each(function () {
                if ($(this).hasClass('tablesorter-headerDesc'))
                    $(this).children('.tablesorter-header-inner').append('<i class="icon-sort-up pull-right">');

                else if ($(this).hasClass('tablesorter-headerAsc'))
                    $(this).children('.tablesorter-header-inner').append('<i class="icon-sort-down pull-right">');

                else if (!$(this).hasClass('sorter-false'))
                    $(this).children('.tablesorter-header-inner').append('<i class="icon-sort pull-right">');
            });
        },
        updatePageNumber: function(event, stuff) {

        }
    });
});