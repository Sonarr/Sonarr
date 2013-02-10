'use strict';
/*global NzbDrone, Backbone*/
/// <reference path="../../app.js" />
/// <reference path="../SeriesModel.js" />
/// <reference path="../SeriesCollection.js" />
/// <reference path="../EditSeriesView.js" />
/// <reference path="../DeleteSeriesView.js" />
/// <reference path="../../Quality/qualityProfileCollection.js" />

NzbDrone.Series.Index.SeriesItemView = Backbone.Marionette.ItemView.extend({
    template: 'Series/Index/SeriesItemTemplate',
    tagName: 'tr',

    ui: {
        'progressbar': '.progress .bar',
    },

    events: {
        'click .x-edit': 'editSeries',
        'click .x-remove': 'removeSeries'
    },

    initialize: function(options) {
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
    onSave: function() {
        alert("saved!");
    }
});

NzbDrone.Series.Index.SeriesCollectionView = Backbone.Marionette.CompositeView.extend({
    itemView: NzbDrone.Series.Index.SeriesItemView,
    itemViewOptions: {},
    template: 'Series/Index/SeriesCollectionTemplate',
    tagName: 'table',
    className: 'table table-hover',
    qualityProfileCollection: new NzbDrone.Quality.QualityProfileCollection(),
    
    initialize: function() {
        this.qualityProfileCollection.fetch();
        this.itemViewOptions = { qualityProfiles: this.qualityProfileCollection };
    }
});
