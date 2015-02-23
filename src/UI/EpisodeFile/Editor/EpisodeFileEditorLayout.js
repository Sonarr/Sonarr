var _ = require('underscore');
var reqres = require('../../reqres');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var FormatHelpers = require('../../Shared/FormatHelpers');
var SelectAllCell = require('../../Cells/SelectAllCell');
var EpisodeNumberCell = require('../../Series/Details/EpisodeNumberCell');
var SeasonEpisodeNumberCell = require('../../Cells/EpisodeNumberCell');
var EpisodeFilePathCell = require('../../Cells/EpisodeFilePathCell');
var EpisodeStatusCell = require('../../Cells/EpisodeStatusCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var EpisodeCollection = require('../../Series/EpisodeCollection');
var ProfileSchemaCollection = require('../../Settings/Profile/ProfileSchemaCollection');
var QualitySelectView = require('./QualitySelectView');
var EmptyView = require('./EmptyView');

module.exports = Marionette.Layout.extend({
    className : 'modal-lg',
    template  : 'EpisodeFile/Editor/EpisodeFileEditorLayoutTemplate',

    regions : {
        episodeGrid : '.x-episode-list',
        quality     : '.x-quality'
    },

    ui : {
        seasonMonitored : '.x-season-monitored'
    },

    events : {
        'click .x-season-monitored' : '_seasonMonitored',
        'click .x-delete-files'     : '_deleteFiles'
    },

    initialize : function(options) {
        if (!options.series) {
            throw 'series is required';
        }

        if (!options.episodeCollection) {
            throw 'episodeCollection is required';
        }

        var filtered = options.episodeCollection.filter(function(episode) {
            return episode.get('episodeFileId') > 0;
        });

        this.series = options.series;
        this.episodeCollection = options.episodeCollection;
        this.filteredEpisodes = new EpisodeCollection(filtered);

        this.templateHelpers = {};
        this.templateHelpers.series = this.series.toJSON();

        this._getColumns();
    },

    onRender : function() {
        this._getQualities();
        this._showEpisodes();
    },

    _getColumns : function () {
        var episodeCell = {};

        if (this.model) {
            episodeCell.name = 'episodeNumber';
            episodeCell.label = '#';
            episodeCell.cell = EpisodeNumberCell;
        }

        else {
            episodeCell.name = 'seasonEpisode';
            episodeCell.cellValue = 'this';
            episodeCell.label = 'Episode';
            episodeCell.cell = SeasonEpisodeNumberCell;
            episodeCell.sortValue = this._seasonEpisodeSorter;
        }

        this.columns = [
            {
                name       : '',
                cell       : SelectAllCell,
                headerCell : 'select-all',
                sortable   : false
            },
            episodeCell,
            {
                name     : 'episodeNumber',
                label    : 'Relative Path',
                cell     : EpisodeFilePathCell,
                sortable : false
            },
            {
                name  : 'airDateUtc',
                label : 'Air Date',
                cell  : RelativeDateCell
            },
            {
                name     : 'status',
                label    : 'Quality',
                cell     : EpisodeStatusCell,
                sortable : false
            }
        ];
    },

    _showEpisodes : function() {
        if (this.filteredEpisodes.length === 0) {
            this.episodeGrid.show(new EmptyView());
            return;
        }

        this._setInitialSort();

        this.episodeGridView = new Backgrid.Grid({
            columns    : this.columns,
            collection : this.filteredEpisodes,
            className  : 'table table-hover season-grid'
        });

        this.episodeGrid.show(this.episodeGridView);
    },

    _setInitialSort : function () {
        if (!this.model) {
            this.filteredEpisodes.setSorting('seasonEpisode', 1, { sortValue: this._seasonEpisodeSorter });
            this.filteredEpisodes.fullCollection.sort();
        }
    },

    _getQualities : function() {
        var self = this;

        var profileSchemaCollection = new ProfileSchemaCollection();
        var promise = profileSchemaCollection.fetch();

        promise.done(function() {
            var profile = profileSchemaCollection.first();

            self.qualitySelectView = new QualitySelectView({ qualities: _.map(profile.get('items'), 'quality') });
            self.listenTo(self.qualitySelectView, 'seasonedit:quality', self._changeQuality);

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

        var selected = this._getSelectedEpisodeFileIds();

        _.each(selected, function(episodeFileId) {
            if (reqres.hasHandler(reqres.Requests.GetEpisodeFileById)) {
                var episodeFile = reqres.request(reqres.Requests.GetEpisodeFileById, episodeFileId);
                episodeFile.set('quality', newQuality);
                episodeFile.save();
            }
        });
    },

    _deleteFiles : function() {
        if (!window.confirm('Are you sure you want to delete the episode files for the selected episodes?')) {
            return;
        }

        var selected = this._getSelectedEpisodeFileIds();

        _.each(selected, function(episodeFileId) {
            if (reqres.hasHandler(reqres.Requests.GetEpisodeFileById)) {
                var episodeFile = reqres.request(reqres.Requests.GetEpisodeFileById, episodeFileId);

                episodeFile.destroy();
            }
        });

        _.each(this.episodeGridView.getSelectedModels(), function(episode) {
            this.episodeGridView.removeRow(episode);
        }, this);
    },

    _getSelectedEpisodeFileIds: function () {
        return _.uniq(_.map(this.episodeGridView.getSelectedModels(), function (episode) {
            return episode.get('episodeFileId');
        }));
    },

    _seasonEpisodeSorter : function (model, attr) {
        var seasonNumber = FormatHelpers.pad(model.get('seasonNumber'), 4, 0);
        var episodeNumber = FormatHelpers.pad(model.get('episodeNumber'), 4, 0);

        return seasonNumber + episodeNumber;
    }
});
