'use strict';
define(
    [
        'marionette',
        'Rename/RenamePreviewItemView'
    ], function (Marionette, RenamePreviewItemView) {
        return Marionette.CollectionView.extend({

            itemView : RenamePreviewItemView
        });
    });
