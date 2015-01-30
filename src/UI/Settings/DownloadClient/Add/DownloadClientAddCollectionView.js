'use strict';

define([
    'Settings/ThingyAddCollectionView',
    'Settings/ThingyHeaderGroupView',
    'Settings/DownloadClient/Add/DownloadClientAddItemView'
], function (ThingyAddCollectionView, ThingyHeaderGroupView, AddItemView) {

    return ThingyAddCollectionView.extend({
        itemView         : ThingyHeaderGroupView.extend({ itemView: AddItemView }),
        itemViewContainer: '.add-download-client .items',
        template         : 'Settings/DownloadClient/Add/DownloadClientAddCollectionViewTemplate'
    });
});
