var Marionette = require('marionette');
var UpdateItemView = require('./UpdateItemView');
var EmptyView = require('./EmptyView');

module.exports = Marionette.CollectionView.extend({
    itemView  : UpdateItemView,
    emptyView : EmptyView
});