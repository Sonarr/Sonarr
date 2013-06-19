'use strict';

define([
    'app',
    'Series/SeriesCollection',
    'Series/Edit/EditSeriesView',
    'Series/Delete/DeleteSeriesView'

], function () {

    NzbDrone.Series.Index.Posters.ItemView = Backbone.Marionette.ItemView.extend({
        tagName : 'li',
        template: 'Series/Index/Posters/ItemTemplate',


        ui: {
            'progressbar': '.progress .bar',
            'controls': '.series-controls'
        },

        events: {
            'click .x-edit'  : 'editSeries',
            'click .x-remove': 'removeSeries',
            'mouseenter .x-series-poster': 'posterHoverAction',
            'mouseleave .x-series-poster': 'posterHoverAction'
        },


        editSeries: function () {
            var view = new NzbDrone.Series.Edit.EditSeriesView({ model: this.model});
            NzbDrone.modalRegion.show(view);
        },

        removeSeries: function () {
            var view = new NzbDrone.Series.Delete.DeleteSeriesView({ model: this.model });
            NzbDrone.modalRegion.show(view);
        },

        posterHoverAction: function () {
            this.ui.controls.slideToggle();
        }
    });

    return NzbDrone.Series.Index.Posters.ItemView;
});
