var Marionette = require('marionette');
var ErrorModel = require('./ErrorModel');

module.exports = Marionette.ItemView.extend({
    template : 'Shared/ErrorViewTemplate',

    initialize: function(data) {
        this.model = new ErrorModel(data);
    }
});