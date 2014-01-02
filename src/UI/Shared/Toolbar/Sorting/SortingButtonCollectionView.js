'use strict';
define(
    [
        'backbone.pageable',
        'marionette',
        'Shared/Toolbar/Sorting/SortingButtonView'
    ], function (PageableCollection, Marionette, ButtonView) {
        return Marionette.CompositeView.extend({
            itemView         : ButtonView,
            template         : 'Shared/Toolbar/Sorting/SortingButtonCollectionViewTemplate',
            itemViewContainer: '.dropdown-menu',

            initialize: function (options) {
                this.viewCollection = options.viewCollection;
                this.listenTo(this.viewCollection, 'drone:sort', this.sort);
            },

            itemViewOptions: function () {
                return {
                    viewCollection: this.viewCollection
                };
            },

            sort: function (sortModel, sortDirection) {
                var collection = this.viewCollection;

                var order;
                if (sortDirection === 'ascending') {
                    order = -1;
                }
                else if (sortDirection === 'descending') {
                    order = 1;
                }
                else {
                    order = null;
                }

                var comparator = this.makeComparator(sortModel.get('name'), order,
                    order ?
                        sortModel.sortValue() :
                        function (model) {
                            return model.cid;
                        });

                if (PageableCollection &&
                    collection instanceof PageableCollection) {

                    collection.setSorting(order && sortModel.get('name'), order,
                        {sortValue: sortModel.sortValue()});

                    if (collection.mode === 'client') {
                        if (collection.fullCollection.comparator === null) {
                            collection.fullCollection.comparator = comparator;
                        }
                        collection.fullCollection.sort();
                    }
                    else {
                        collection.fetch({reset: true});
                    }
                }
                else {
                    collection.comparator = comparator;
                    collection.sort();
                }

                return this;
            },

            makeComparator: function (attr, order, func) {

                return function (left, right) {
                    // extract the values from the models
                    var l = func(left, attr), r = func(right, attr), t;

                    // if descending order, swap left and right
                    if (order === 1) t = l, l = r, r = t;

                    // compare as usual
                    if (l === r) return 0;
                    else if (l < r) return -1;
                    return 1;
                };
            }
        });
    });


