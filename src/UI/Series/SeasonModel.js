var Backbone = require('backbone');

module.exports = Backbone.Model.extend({
    defaults : {
        seasonNumber : 0
    },

    initialize : function() {
        this.set('id', this.get('seasonNumber'));
    }
});