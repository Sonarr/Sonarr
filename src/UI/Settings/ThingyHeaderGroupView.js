var Backbone = require('backbone');
var Marionette = require('marionette');

module.exports = Marionette.CompositeView.extend({
    itemViewContainer : '.item-list',
    template          : 'Settings/ThingyHeaderGroupViewTemplate',
    tagName           : 'div',

    itemViewOptions : function() {
        return {
            targetCollection : this.targetCollection || this.options.targetCollection
        };
    },

    initialize : function() {
        this.collection = new Backbone.Collection(this.model.get('collection'));
    }
});