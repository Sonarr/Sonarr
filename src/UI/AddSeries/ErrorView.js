var Marionette = require('marionette');

module.exports = Marionette.CompositeView.extend({
    template : 'AddSeries/ErrorViewTemplate',

    initialize : function(options) {
        this.options = options;
    },

    templateHelpers : function() {
        return this.options;
    }
});