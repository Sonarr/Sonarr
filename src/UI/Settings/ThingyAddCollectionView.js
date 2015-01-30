'use strict';

define([
    'marionette'
], function (Marionette) {

    return Marionette.CompositeView.extend({
        itemViewOptions  : function () {
            return {
                targetCollection: this.targetCollection || this.options.targetCollection
            };
        },

        initialize: function (options) {
            this.targetCollection = options.targetCollection;
        }
    });
});
