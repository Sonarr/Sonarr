'use strict';
define(['marionette', 'Mixins/AsModelBoundView'], function (Marionette, AsModelBoundView) {
    var view = Marionette.ItemView.extend({
            template: 'Settings/General/GeneralTemplate'
        }
    );

    return AsModelBoundView.call(view);
});

