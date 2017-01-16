var Backbone = require('backbone');
var ButtonModel = require('./ButtonModel');

module.exports = Backbone.Collection.extend({
    model : ButtonModel
});