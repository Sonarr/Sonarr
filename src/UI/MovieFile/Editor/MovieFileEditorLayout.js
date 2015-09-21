var _ = require('underscore');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var FormatHelpers = require('../../Shared/FormatHelpers');
var SelectAllCell = require('../../Cells/SelectAllCell');
var MovieFilePathCell = require('../../Cells/MovieFilePathCell');
var MovieStatusCell = require('../../Cells/MovieStatusCell');
var ProfileSchemaCollection = require('../../Settings/Profile/ProfileSchemaCollection');
var QualitySelectView = require('./QualitySelectView');
var EmptyView = require('./EmptyView');

module.exports = Marionette.Layout.extend({
    className : 'modal-lg',
    template  : 'MovieFile/Editor/MovieFileEditorLayoutTemplate',

    regions : {
        movieGrid   : '.x-movie-list',
        quality     : '.x-quality'
    },

    ui : {
        seasonMonitored : '.x-season-monitored'
    },

    events : {
        'click .x-delete-files'     : '_deleteFiles'
    },

    initialize : function(options) {
        if (!options.movie) {
            throw 'movie is required';
        }

        if (!options.movieFileCollection) {
            throw 'movieFile is required';
        }

        this.movie = options.movie;
        this.movieFileCollection = options.movieFileCollection;

        this.templateHelpers = {};
        this.templateHelpers.movie = this.movie.toJSON();

        this._getColumns();
    },

    onRender : function() {
        this._getQualities();
        this._showMovieFiles();
    },

    _getColumns : function () {
        this.columns = [
            {
                name       : '',
                cell       : SelectAllCell,
                headerCell : 'select-all',
                sortable   : false
            },
            {
                name     : 'Path',
                label    : 'Relative Path',
                cell     : MovieFilePathCell,
                sortable : false
            },
            {
                name     : 'status',
                label    : 'Quality',
                cell     : MovieStatusCell,
                sortable : false
            }
        ];
    },

    _showMovieFiles : function() {
        if (this.movieFileCollection.length === 0) {
            this.movieGrid.show(new EmptyView());
            return;
        }

        this.movieGridView = new Backgrid.Grid({
            columns    : this.columns,
            collection : this.movieFileCollection,
            className  : 'table table-hover season-grid'
        });

        this.movieGrid.show(this.movieGridView);
    },

    _getQualities : function() {
        var self = this;

        var profileSchemaCollection = new ProfileSchemaCollection();
        var promise = profileSchemaCollection.fetch();

        promise.done(function() {
            var profile = profileSchemaCollection.first();

            self.qualitySelectView = new QualitySelectView({ qualities: _.map(profile.get('items'), 'quality') });
            self.listenTo(self.qualitySelectView, 'movieedit:quality', self._changeQuality);

            self.quality.show(self.qualitySelectView);
        });
    },

    _changeQuality : function(options) {
        var newQuality = {
            quality  : options.selected,
            revision : {
                version : 1,
                real    : 0
            }
        };

        var selected = this._getSelectedMovieFileIds();

        _.each(selected, function(movieFileId) {
            var movieFile = this.movieFileCollection.get(movieFileId);
            movieFile.set('quality', newQuality);
            movieFile.save();
        }, this);
    },

    _deleteFiles : function() {
        if (!window.confirm('Are you sure you want to delete the movie files selected?')) {
            return;
        }

        var selected = this._getSelectedMovieFileIds();

        _.each(selected, function(movieFileId) {
            var movieFile = this.movieFileCollection.get(movieFileId);
            movieFile.destroy();
        }, this);

        _.each(this.movieGridView.getSelectedModels(), function(movie) {
            this.movieGridView.removeRow(movie);
        }, this);
    },

    _getSelectedMovieFileIds: function () {
        return _.uniq(_.map(this.movieGridView.getSelectedModels(), function (movieFile) {
            return movieFile.get('id');
        }));
    }
});
