'use strict';
/*global NzbDrone, Backbone*/
/// <reference path="../../app.js" />
/// <reference path="../SeriesModel.js" />
/// <reference path="../SeriesCollection.js" />
/// <reference path="../DeleteSeriesView.js" />

NzbDrone.Series.Index.SeriesItemView = Backbone.Marionette.ItemView.extend({
    template: 'Series/Index/SeriesItemTemplate',
    tagName: 'tr',

    ui: {
        'progressbar': '.progress .bar',
    },

    events: {
        'click .x-remove': 'removeSeries',
    },

    onRender: function () {
        NzbDrone.ModelBinder.bind(this.model, this.el);
    },

    removeSeries: function () {
        //this.model.destroy({ wait: true });
        //this.model.collection.remove(this.model);

        var view = new NzbDrone.Series.DeleteSeriesView({ model: this.model });
        NzbDrone.modalRegion.show(view);
    },
});

NzbDrone.Series.Index.SeriesCollectionView = Backbone.Marionette.CompositeView.extend({
    itemView: NzbDrone.Series.Index.SeriesItemView,
    template: 'Series/Index/SeriesCollectionTemplate',
    tagName: 'table',
    className: 'table table-hover',
});
