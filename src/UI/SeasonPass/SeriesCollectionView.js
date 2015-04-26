var Marionette = require('marionette');
var SeriesLayout = require('./SeriesLayout');

module.exports = Marionette.CollectionView.extend({
    itemView : SeriesLayout
});