var Marionette = require('marionette');
var commandCollection = require('./CommandCollection');
var CommandMessengerItemView = require('./CommandMessengerItemView');

var CollectionView = Marionette.CollectionView.extend({
    itemView : CommandMessengerItemView
});

module.exports = new CollectionView({
    collection : commandCollection
});
