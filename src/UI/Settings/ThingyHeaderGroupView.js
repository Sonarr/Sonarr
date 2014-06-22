'use strict';

define([
    'backbone',
    'marionette'
], function (Backbone, Marionette) {

    return Marionette.CompositeView.extend({
        itemViewContainer: '.item-list',
        template: 'Settings/ThingyHeaderGroupViewTemplate',
        tagName : 'div',
            
        itemViewOptions: function () {
           return {
               targetCollection: this.targetCollection || this.options.targetCollection
           };
        },
            
        initialize: function () {
            this.collection = new Backbone.Collection(this.model.get('collection'));
        }
    });
});
