var reqres = require('../../../reqres');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var MovieFileModel = require('../../MovieFileModel');
var FileSizeCell = require('../../../Cells/FileSizeCell');
var QualityCell = require('../../../Cells/QualityCell');
var DeleteMovieFileCell = require('../../../Cells/DeleteMovieFileCell');
var NoFileView = require('./NoFileView');
var LoadingView = require('../../../Shared/LoadingView');
require('../../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'Movie/Modal/Summary/MovieSummaryLayoutTemplate',

    regions : {
        overview : '.movie-overview',
        activity : '.movie-file-info'
    },

    columns : [
        {
            name     : 'path',
            label    : 'Path',
            cell     : 'string',
            sortable : false
        },
        {
            name     : 'size',
            label    : 'Size',
            cell     : FileSizeCell,
            sortable : false
        },
        {
            name     : 'quality',
            label    : 'Quality',
            cell     : QualityCell,
            sortable : false,
            editable : true
        },
        {
            name     : 'this',
            label    : '',
            cell     : DeleteMovieFileCell,
            sortable : false
        }
    ],

    templateHelpers : {},

    initialize : function(options) {
        this.movieFileCollection = options.movieFileCollection;
        this.movieFileCollection.fetch();
        this.listenTo(this.movieFileCollection, 'sync', this._syncDone);
    },

    _syncDone : function (){
        if (this.movieFileCollection.length === 1) {
            this.movieFile = this.movieFileCollection.get(this.model.get('movieFileId'));
            this.listenTo(this.movieFileCollection, 'add remove', this._collectionChanged);
            this._showTable();
        }
        else {
            this._showNoFileView();
        }
    },

    _showTable : function() {
        this.activity.show(new Backgrid.Grid({
            collection : this.movieFileCollection,
            columns    : this.columns,
            className  : 'table table-bordered',
            emptyText  : 'Nothing to see here!'
        }));
    },

    _showNoFileView : function() {
        this.activity.show(new NoFileView());
    },

    _collectionChanged : function() {
        if (!this.movieFileCollection.any()) {
            this._showNoFileView();
        }

        else {
            this._showTable();
        }
    },

    _movieFileDeleted : function() {
        this.model.set({
            movieFileId : 0,
            hasFile     : false
        });
    }
});