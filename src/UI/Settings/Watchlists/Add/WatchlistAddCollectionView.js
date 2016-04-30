var ThingyAddCollectionView = require('../../ThingyAddCollectionView');
var AddItemView = require('./WatchlistAddItemView');

module.exports = ThingyAddCollectionView.extend({
	itemView          : AddItemView,
    itemViewContainer : '.add-watchlists .items',
    template          : 'Settings/Watchlists/Add/WatchlistAddCollectionViewTemplate'
});
