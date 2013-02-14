'use strict';

define(['app', 'Series/SeriesModel'], function () {

    NzbDrone.Series.DeleteSeriesView = Backbone.Marionette.ItemView.extend({
        template: 'Series/Delete/DeleteSeriesTemplate',
        tagName: 'div',
        className: "modal",

        ui: {
            'progressbar': '.progress .bar',
        },

        events: {
            'click .x-confirm-delete': 'removeSeries',
        },

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        },

        removeSeries: function () {
            //Todo: why the fuck doesn't destroy send the ID?
            this.model.destroy({ wait: true, headers: { id: this.model.get('id'), deleteFiles: $('#delete-from-disk').prop('checked') } });
            this.model.collection.remove(this.model);
            this.$el.parent().modal('hide');
        },
    });
});