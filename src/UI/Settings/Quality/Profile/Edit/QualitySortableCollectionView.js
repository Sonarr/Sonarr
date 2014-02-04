'use strict';
define(
    [
        'backbone.collectionview',
        'Settings/Quality/Profile/Edit/EditQualityProfileItemView'
    ], function (BackboneSortableCollectionView, EditQualityProfileItemView) {
        return BackboneSortableCollectionView.extend({

            className: 'qualities',
            modelView: EditQualityProfileItemView,

            attributes: {
                'validation-name': 'items'
            },

            events: {
                'click li, td' : '_listItem_onMousedown',
                'dblclick li, td'  : '_listItem_onDoubleClick',
                'keydown'          : '_onKeydown'
            }
        });
    });
