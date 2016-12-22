var BackboneSortableCollectionView = require('backbone.collectionview');
var EditLanguageProfileItemView = require('./EditLanguageProfileItemView');

module.exports = BackboneSortableCollectionView.extend({
    className : 'qualities',
    modelView : EditLanguageProfileItemView,

    attributes : {
        'validation-name' : 'languages'
    },

    events : {
        'click li, td'    : '_listItem_onMousedown',
        'dblclick li, td' : '_listItem_onDoubleClick',
        'keydown'         : '_onKeydown'
    }
});