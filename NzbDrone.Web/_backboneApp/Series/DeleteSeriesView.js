'use strict';
/*global NzbDrone, Backbone*/
/// <reference path="../app.js" />
/// <reference path="SeriesModel.js" />

NzbDrone.Series.DeleteSeriesView = Backbone.Marionette.ItemView.extend({
    template: 'Series/DeleteSeriesTemplate',
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
        this.model.destroy({ wait: true });
        this.model.collection.remove(this.model);
        this.$el.parent().modal('hide');
    },
});