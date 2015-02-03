var PageableCollection = require('backbone.pageable');
var Marionette = require('marionette');
var ButtonView = require('./SortingButtonView');

module.exports = Marionette.CompositeView.extend({
    itemView          : ButtonView,
    template          : 'Shared/Toolbar/Sorting/SortingButtonCollectionViewTemplate',
    itemViewContainer : '.dropdown-menu',
    initialize        : function(options){
        this.viewCollection = options.viewCollection;
        this.listenTo(this.viewCollection, 'drone:sort', this.sort);
    },
    itemViewOptions   : function(){
        return {viewCollection : this.viewCollection};
    },
    sort              : function(sortModel, sortDirection){
        var collection = this.viewCollection;
        var order;
        if(sortDirection === 'ascending') {
            order = -1;
        }
        else if(sortDirection === 'descending') {
            order = 1;
        }
        else {
            order = null;
        }
        collection.setSorting(sortModel.get('name'), order);
        collection.fullCollection.sort();
        return this;
    }
});