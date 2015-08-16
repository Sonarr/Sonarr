var Marionette = require('marionette');

module.exports = Marionette.CompositeView.extend({
    template : 'AddMovie/NotFoundViewTemplate',

    initialize : function(options) {
        this.options = options;
    },

    templateHelpers : function() {
        return this.options;
    }
});