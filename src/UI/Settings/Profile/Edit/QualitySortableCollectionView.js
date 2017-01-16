var BackboneSortableCollectionView = require('backbone.collectionview');
var EditProfileItemView = require('./EditProfileItemView');

module.exports = BackboneSortableCollectionView.extend({
    className : 'qualities',
    modelView : EditProfileItemView,

    attributes : {
        'validation-name' : 'items'
    },

    events : {
        'click li, td'    : '_listItem_onMousedown',
        'dblclick li, td' : '_listItem_onDoubleClick',
        'keydown'         : '_onKeydown'
    }
});