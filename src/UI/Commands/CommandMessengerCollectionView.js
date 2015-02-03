var Marionette = require('marionette');
var commandCollection = require('./CommandCollection');
var CommandMessengerItemView = require('./CommandMessengerItemView');

module.exports = (function(){
    var CollectionView = Marionette.CollectionView.extend({itemView : CommandMessengerItemView});
    return new CollectionView({collection : commandCollection});
}).call(this);