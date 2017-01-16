module.exports = function() {
    this.prototype.appendHtml = function(collectionView, itemView, index) {
        var childrenContainer = collectionView.itemViewContainer ? collectionView.$(collectionView.itemViewContainer) : collectionView.$el;
        var collection = collectionView.collection;

        // If the index of the model is at the end of the collection append, else insert at proper index
        if (index >= collection.size() - 1) {
            childrenContainer.append(itemView.el);
        } else {
            var previousModel = collection.at(index + 1);
            var previousView = this.children.findByModel(previousModel);

            if (previousView) {
                previousView.$el.before(itemView.$el);
            }

            else {
                childrenContainer.append(itemView.el);
            }
        }
    };

    return this;
};