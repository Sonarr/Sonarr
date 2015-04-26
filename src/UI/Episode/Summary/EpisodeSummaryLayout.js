var reqres = require('../../reqres');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var EpisodeFileModel = require('../../Series/EpisodeFileModel');
var EpisodeFileCollection = require('../../Series/EpisodeFileCollection');
var FileSizeCell = require('../../Cells/FileSizeCell');
var QualityCell = require('../../Cells/QualityCell');
var DeleteEpisodeFileCell = require('../../Cells/DeleteEpisodeFileCell');
var NoFileView = require('./NoFileView');
var LoadingView = require('../../Shared/LoadingView');

module.exports = Marionette.Layout.extend({
    template : 'Episode/Summary/EpisodeSummaryLayoutTemplate',

    regions : {
        overview : '.episode-overview',
        activity : '.episode-file-info'
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
            cell     : DeleteEpisodeFileCell,
            sortable : false
        }
    ],

    templateHelpers : {},

    initialize : function(options) {
        if (!this.model.series) {
            this.templateHelpers.series = options.series.toJSON();
        }
    },

    onShow : function() {
        if (this.model.get('hasFile')) {
            var episodeFileId = this.model.get('episodeFileId');

            if (reqres.hasHandler(reqres.Requests.GetEpisodeFileById)) {
                var episodeFile = reqres.request(reqres.Requests.GetEpisodeFileById, episodeFileId);
                this.episodeFileCollection = new EpisodeFileCollection(episodeFile, { seriesId : this.model.get('seriesId') });
                this.listenTo(episodeFile, 'destroy', this._episodeFileDeleted);

                this._showTable();
            }

            else {
                this.activity.show(new LoadingView());

                var self = this;
                var newEpisodeFile = new EpisodeFileModel({ id : episodeFileId });
                this.episodeFileCollection = new EpisodeFileCollection(newEpisodeFile, { seriesId : this.model.get('seriesId') });
                var promise = newEpisodeFile.fetch();
                this.listenTo(newEpisodeFile, 'destroy', this._episodeFileDeleted);

                promise.done(function() {
                    self._showTable();
                });
            }

            this.listenTo(this.episodeFileCollection, 'add remove', this._collectionChanged);
        }

        else {
            this._showNoFileView();
        }
    },

    _showTable : function() {
        this.activity.show(new Backgrid.Grid({
            collection : this.episodeFileCollection,
            columns    : this.columns,
            className  : 'table table-bordered',
            emptyText  : 'Nothing to see here!'
        }));
    },

    _showNoFileView : function() {
        this.activity.show(new NoFileView());
    },

    _collectionChanged : function() {
        if (!this.episodeFileCollection.any()) {
            this._showNoFileView();
        }

        else {
            this._showTable();
        }
    },

    _episodeFileDeleted : function() {
        this.model.set({
            episodeFileId : 0,
            hasFile       : false
        });
    }
});