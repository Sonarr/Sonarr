'use strict';
define(
    [
        'vent',
        'marionette',
        'Mixins/AsModelBoundView'
    ], function (vent, Marionette, AsModelBoundView) {
        var view = Marionette.ItemView.extend({
            template: 'Rename/RenamePreviewItemViewTemplate'
        });

        return AsModelBoundView.apply(view);
    });
