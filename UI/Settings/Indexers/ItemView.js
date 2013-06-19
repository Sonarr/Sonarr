"use strict";

define(['marionette', 'Mixins/AsModelBoundView'], function (Marionette, AsModelBoundView) {

    var view = Marionette.ItemView.extend({
        template: 'Settings/Indexers/ItemTemplate',
        tagName : 'li'
    });

    return AsModelBoundView.call(view);

});
