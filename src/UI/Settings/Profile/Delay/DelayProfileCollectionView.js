'use strict';
define([
    'backbone.collectionview',
    'Settings/Profile/Delay/DelayProfileItemView'
], function (BackboneSortableCollectionView, DelayProfileItemView) {

    return BackboneSortableCollectionView.extend({
        className : 'delay-profiles',
        modelView : DelayProfileItemView,

        events: {
            'click li, td'    : '_listItem_onMousedown',
            'dblclick li, td' : '_listItem_onDoubleClick',
            'keydown'         : '_onKeydown'
        }
    });
});
