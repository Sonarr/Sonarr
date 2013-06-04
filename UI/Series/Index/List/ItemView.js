'use strict';

define([
    'app',
    'Quality/QualityProfileCollection',
    'Series/SeriesCollection',
    'Series/Edit/EditSeriesView',
    'Series/Delete/DeleteSeriesView'

], function () {

    NzbDrone.Series.Index.List.ItemView = Backbone.Marionette.ItemView.extend({
        template: 'Series/Index/List/ItemTemplate',

        ui: {
            'progressbar': '.progress .bar'
        },

        events: {
            'click .x-edit'  : 'editSeries',
            'click .x-remove': 'removeSeries',
        },

        initialize: function (options) {
            this.qualityProfileCollection = options.qualityProfiles;
        },

        editSeries: function () {
            var view = new NzbDrone.Series.Edit.EditSeriesView({ model: this.model});
            NzbDrone.modalRegion.show(view);
        },

        removeSeries: function () {
            var view = new NzbDrone.Series.Delete.DeleteSeriesView({ model: this.model });
            NzbDrone.modalRegion.show(view);
        }
    });
});
