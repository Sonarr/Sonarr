var SeriesIndexItemView = require('../SeriesIndexItemView');

module.exports = SeriesIndexItemView.extend({
    tagName  : 'li',
    template : 'Series/Index/Posters/SeriesPostersItemViewTemplate',

    initialize : function() {
        this.events['mouseenter .x-series-poster-container'] = 'posterHoverAction';
        this.events['mouseleave .x-series-poster-container'] = 'posterHoverAction';

        this.ui.controls = '.x-series-controls';
        this.ui.title = '.x-title';
    },

    posterHoverAction : function() {
        this.ui.controls.slideToggle();
        this.ui.title.slideToggle();
    }
});