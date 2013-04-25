'use strict';

define([
    'app',
    'Quality/QualityProfileCollection',
    'Series/SeriesCollection',
    'Series/Edit/EditSeriesView',
    'Series/Delete/DeleteSeriesView'

], function () {

    NzbDrone.Series.Index.Posters.ItemView = Backbone.Marionette.ItemView.extend({
        tagName : 'li',
        template: 'Series/Index/Posters/ItemTemplate',


        ui: {
            'progressbar': '.progress .bar',
            'airDate': '.air-date',
            'controls': '.series-controls'
        },

        events: {
            'click .x-edit'  : 'editSeries',
            'click .x-remove': 'removeSeries',
            'mouseenter .x-series-poster': 'posterHoverAction',
            'mouseleave .x-series-poster': 'posterHoverAction'
        },

        initialize: function (options) {
            this.qualityProfileCollection = options.qualityProfiles;
        },

        onRender: function () {
            this.ui.airDate.tooltip();
        },

        editSeries: function () {
            var view = new NzbDrone.Series.Edit.EditSeriesView({ model: this.model});

            NzbDrone.vent.trigger(NzbDrone.Events.OpenModalDialog, {
                view: view
            });
        },

        removeSeries: function () {
            var view = new NzbDrone.Series.Delete.DeleteSeriesView({ model: this.model });
            NzbDrone.vent.trigger(NzbDrone.Events.OpenModalDialog, {
                view: view
            });
        },

        posterHoverAction: function () {
            this.ui.controls.slideToggle();
        }
    });
});
