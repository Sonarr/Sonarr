var Marionette = require('marionette');
var PosterItemView = require('./SeriesPostersItemView');

module.exports = Marionette.CompositeView.extend({
    itemView          : PosterItemView,
    itemViewContainer : '#x-series-posters',
    template          : 'Series/Index/Posters/SeriesPostersCollectionViewTemplate'
});