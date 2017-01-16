var ThingyAddCollectionView = require('../../ThingyAddCollectionView');
var ThingyHeaderGroupView = require('../../ThingyHeaderGroupView');
var AddItemView = require('./IndexerAddItemView');

module.exports = ThingyAddCollectionView.extend({
    itemView          : ThingyHeaderGroupView.extend({ itemView : AddItemView }),
    itemViewContainer : '.add-indexer .items',
    template          : 'Settings/Indexers/Add/IndexerAddCollectionViewTemplate'
});