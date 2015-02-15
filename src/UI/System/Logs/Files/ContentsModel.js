var Backbone = require('backbone');

module.exports = Backbone.Model.extend({
    url : function() {
        return this.get('contentsUrl');
    },

    parse : function(contents) {
        var response = {};
        response.contents = contents;
        return response;
    }
});