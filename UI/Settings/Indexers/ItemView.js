"use strict";

define(['marionette'], function () {

    return Marionette.ItemView.extend({
        template: 'Settings/Indexers/ItemTemplate',
        tagName : 'li'
    });

});
