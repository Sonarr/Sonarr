'use strict';

define(['marionette', 'Mixins/AsModelBoundview'], function (Marionette, AsModelBoundView) {

    var view = Marionette.ItemView.extend({
        template : 'Settings/Misc/MiscTemplate',
        className: 'form-horizontal',

        ui: {
            tooltip: '[class^='help-inline'] i'
        }

    });

    return AsModelBoundView.call(view);
});
