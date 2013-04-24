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
            'progressbar': '.progress .bar'
        },

        events: {
            'click .x-edit'  : 'editSeries',
            'click .x-remove': 'removeSeries'
        },

        initialize: function (options) {
            this.qualityProfileCollection = options.qualityProfiles;
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
        }
    });
});
