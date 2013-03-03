'use strict';

define([
        'app',
        'Quality/QualityProfileCollection',
        'Series/SeriesCollection',
        'Series/Edit/EditSeriesView',
        'Series/Delete/DeleteSeriesView'

], function () {

    NzbDrone.Series.Index.SeriesItemView = Backbone.Marionette.ItemView.extend({
        template: 'Series/Index/SeriesItemTemplate',
        tagName: 'tr',

        ui: {
            'progressbar': '.progress .bar'
        },

        events: {
            'click .x-edit': 'editSeries',
            'click .x-remove': 'removeSeries'
        },

        initialize: function (options) {
            this.qualityProfileCollection = options.qualityProfiles;
        },

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        },


        editSeries: function () {
            var view = new NzbDrone.Series.EditSeriesView({ model: this.model});

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
