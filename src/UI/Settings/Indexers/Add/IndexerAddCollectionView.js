'use strict';

define([
    'Settings/ThingyAddCollectionView',
    'Settings/ThingyHeaderGroupView',
    'Settings/Indexers/Add/IndexerAddItemView'
], function (ThingyAddCollectionView, ThingyHeaderGroupView, AddItemView) {

    return ThingyAddCollectionView.extend({
        itemView         : ThingyHeaderGroupView.extend({ itemView: AddItemView }),
        itemViewContainer: '.add-indexer .items',
        template         : 'Settings/Indexers/Add/IndexerAddCollectionViewTemplate'
    });
});
