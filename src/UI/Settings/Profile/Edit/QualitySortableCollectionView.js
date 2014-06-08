'use strict';
define(
    [
        'backbone.collectionview',
        'Settings/Profile/Edit/EditProfileItemView'
    ], function (BackboneSortableCollectionView, EditProfileItemView) {
        return BackboneSortableCollectionView.extend({

            className: 'qualities',
            modelView: EditProfileItemView,

            attributes: {
                'validation-name': 'items'
            },

            events: {
                'click li, td'    : '_listItem_onMousedown',
                'dblclick li, td' : '_listItem_onDoubleClick',
                'keydown'         : '_onKeydown'
            }
        });
    });
