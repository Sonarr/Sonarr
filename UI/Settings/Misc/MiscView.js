'use strict';

define(['marionette', 'Mixins/AsModelBoundview', 'bootstrap'], function (Marionette, AsModelBoundView) {

    var view = Marionette.ItemView.extend({
        template : 'Settings/Misc/MiscTemplate',
        className: 'form-horizontal',

        ui: {
            tooltip: '[class^="help-inline"] i'
        },

        onRender: function () {
            this.ui.tooltip.tooltip({ placement: 'right', html: true });
        }
    });

    return AsModelBoundView.call(view);
});
