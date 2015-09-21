var Marionette = require('marionette');
var SearchResultView = require('./SearchResultView');

module.exports = Marionette.CollectionView.extend({
    itemView : SearchResultView,

    initialize : function(options) {
        this.isExisting = options.isExisting;
        this.showing = 1;
    },

    showAll : function() {
        this.showingAll = true;
        this.render();
    },

    showMore : function() {
        this.showing += 5;
        this.render();

        return this.showing >= this.collection.length;
    },

    appendHtml : function(collectionView, itemView, index) {
        if (!this.isExisting || index < this.showing || index === 0) {
            collectionView.$el.append(itemView.el);
        }
    }
});