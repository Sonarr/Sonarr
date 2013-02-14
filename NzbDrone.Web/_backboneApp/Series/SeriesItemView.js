'use strict';

define([
        'app',
        'Series/SeriesCollection',
        'Series/Edit/EditSeriesView',
        'Series/Delete/DeleteSeriesView',
        'Quality/QualityProfileCollection'
], function () {

    NzbDrone.Series.SeriesItemView = Backbone.Marionette.ItemView.extend({
        template: 'Series/SeriesItemTemplate',
        tagName: 'tr',

        ui: {
            'progressbar': '.progress .bar',
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

        qualityProfileCollection: new NzbDrone.Quality.QualityProfileCollection(),

        editSeries: function () {
            var view = new NzbDrone.Series.EditSeriesView({ model: this.model, qualityProfiles: this.qualityProfileCollection });
            view.on('saved', this.render, this);
            NzbDrone.modalRegion.show(view);
        },

        removeSeries: function () {
            var view = new NzbDrone.Series.DeleteSeriesView({ model: this.model });
            NzbDrone.modalRegion.show(view);
        },
        onSave: function () {
            alert("saved!");
        }
    });
});
