'use strict';
define(
    [
        'backbone.collectionview'
    ], function (BackboneSortableCollectionView) {
        return BackboneSortableCollectionView.extend({

            events : {
                'mousedown li, td' : '_listItem_onMousedown',
                'dblclick li, td'  : '_listItem_onDoubleClick',
                'click'            : '_listBackground_onClick',
                'click ul.collection-list, table.collection-list' : '_listBackground_onClick',
                'keydown'          : '_onKeydown',
                'click .x-move'    : '_onClickMove'
            },

            _onClickMove: function( theEvent ) {
                var clickedItemId = this._getClickedItemId( theEvent );

                if( clickedItemId )
                {
                    var clickedModel = this.collection.get( clickedItemId );
                    this.trigger('moveClicked', clickedModel);
                }
            }
        });
    });
