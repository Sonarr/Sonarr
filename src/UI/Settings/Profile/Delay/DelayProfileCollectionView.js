var BackboneSortableCollectionView = require('backbone.collectionview');
var DelayProfileItemView = require('./DelayProfileItemView');

module.exports = BackboneSortableCollectionView.extend({
    className : 'delay-profiles',
    modelView : DelayProfileItemView,

    events : {
        'click li, td'    : '_listItem_onMousedown',
        'dblclick li, td' : '_listItem_onDoubleClick',
        'keydown'         : '_onKeydown'
    }
});