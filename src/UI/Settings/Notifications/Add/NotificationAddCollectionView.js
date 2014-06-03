'use strict';

define([
    'Settings/ThingyAddCollectionView',
    'Settings/Notifications/Add/NotificationAddItemView'
], function (ThingyAddCollectionView, AddItemView) {

    return ThingyAddCollectionView.extend({
        itemView         : AddItemView,
        itemViewContainer: '.add-notifications .items',
        template         : 'Settings/Notifications/Add/NotificationAddCollectionViewTemplate'
    });
});
