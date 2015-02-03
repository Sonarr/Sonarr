var ThingyAddCollectionView = require('../../ThingyAddCollectionView');
var AddItemView = require('./NotificationAddItemView');

module.exports = ThingyAddCollectionView.extend({
    itemView          : AddItemView,
    itemViewContainer : '.add-notifications .items',
    template          : 'Settings/Notifications/Add/NotificationAddCollectionViewTemplate'
});