var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var SeriesCollection = require('../../Series/SeriesCollection');
var SelectRow = require('./SelectSeriesRow');

module.exports = Marionette.Layout.extend({
    template  : 'ManualImport/Series/SelectSeriesLayoutTemplate',

    regions : {
        series : '.x-series'
    },

    ui : {
        filter : '.x-filter'
    },

    columns : [
        {
            name      : 'title',
            label     : 'Title',
            cell      : 'String',
            sortValue : 'sortTitle'
        }
    ],

    initialize : function() {
        this.seriesCollection = SeriesCollection.clone();
        this._setModelCollection();

        this.listenTo(this.seriesCollection, 'row:selected', this._onSelected);
        this.listenTo(this, 'modal:afterShow', this._setFocus);
    },

    onRender : function() {
        this.seriesView = new Backgrid.Grid({
            columns    : this.columns,
            collection : this.seriesCollection,
            className  : 'table table-hover season-grid',
            row        : SelectRow
        });

        this.series.show(this.seriesView);
        this._setupFilter();
    },

    _setupFilter : function () {
        var self = this;

        //TODO: This should be a mixin (same as Add Series searching)
        this.ui.filter.keyup(function(e) {
            if (_.contains([
                    9,
                    16,
                    17,
                    18,
                    19,
                    20,
                    33,
                    34,
                    35,
                    36,
                    37,
                    38,
                    39,
                    40,
                    91,
                    92,
                    93
                ], e.keyCode)) {
                return;
            }

            self._filter(self.ui.filter.val());
        });
    },

    _filter : function (term) {
        this.seriesCollection.setFilter(['title', term, 'contains']);
        this._setModelCollection();
    },

    _onSelected : function (e) {
        this.trigger('manualimport:selected:series', { model: e.model });

        vent.trigger(vent.Commands.CloseModal2Command);
    },

    _setFocus : function () {
        this.ui.filter.focus();
    },
    
    _setModelCollection: function () {
        var self = this;
        
        _.each(this.seriesCollection.models, function (model) {
            model.collection = self.seriesCollection;
        });
    }
});
