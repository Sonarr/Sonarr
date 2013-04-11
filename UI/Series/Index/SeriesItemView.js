'use strict';

define([
    'app',
    'Quality/QualityProfileCollection',
    'Series/SeriesCollection',
    'Series/Edit/EditSeriesView',
    'Series/Delete/DeleteSeriesView'

], function () {

    NzbDrone.Series.Index.SeriesItemView = Backbone.Marionette.ItemView.extend({
        tagName : 'tr',
        template: 'Series/Index/SeriesItemTemplate',

        getTemplate: function(){
            if (this.viewStyle === 1){
                this.tagName = 'div';
                return 'Series/Index/SeriesGridItemTemplate';
            }
            else {
                return 'Series/Index/SeriesItemTemplate';
            }
        },

        ui: {
            'progressbar': '.progress .bar'
        },

        events: {
            'click .x-edit'  : 'editSeries',
            'click .x-remove': 'removeSeries'
        },

        initialize: function (options) {
            this.qualityProfileCollection = options.qualityProfiles;
            this.viewStyle = options.viewStyle;
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
