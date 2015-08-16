var Marionette = require('marionette');
var RenameMoviePreviewItemView = require('./RenameMoviePreviewItemView');

module.exports = Marionette.CollectionView.extend({
    itemView : RenameMoviePreviewItemView
});