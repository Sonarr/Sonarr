'use strict';
define(
    [
        'marionette',
        'Shared/Toolbar/Button/ButtonView'
    ], function (Marionette, ButtonView) {
        return Marionette.CollectionView.extend({
            className: 'btn-group',
            itemView : ButtonView
        });
    });


