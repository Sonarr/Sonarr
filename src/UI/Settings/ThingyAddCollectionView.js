var Marionette = require('marionette');

module.exports = Marionette.CompositeView.extend({
    itemViewOptions : function(){
        return {targetCollection : this.targetCollection || this.options.targetCollection};
    },
    initialize      : function(options){
        this.targetCollection = options.targetCollection;
    }
});