var $ = require('jquery');
var _ = require('underscore');
var vent = require('vent');
var reqres = require('../../reqres');
var Marionette = require('marionette');
var Backbone = require('backbone');
var SeriesCollection = require('../SeriesCollection');
var EpisodeCollection = require('../EpisodeCollection');
var EpisodeFileCollection = require('../EpisodeFileCollection');
var SeasonCollection = require('../SeasonCollection');
var SeasonCollectionView = require('./SeasonCollectionView');
var InfoView = require('./InfoView');
var CommandController = require('../../Commands/CommandController');
var LoadingView = require('../../Shared/LoadingView');
var EpisodeFileEditorLayout = require('../../EpisodeFile/Editor/EpisodeFileEditorLayout');
require('backstrech');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    itemViewContainer : '.x-series-seasons',
    template          : 'Series/Details/SeriesDetailsTemplate',

    regions : {
        seasons : '#seasons',
        info    : '#info'
    },

    ui : {
        header    : '.x-header',
        monitored : '.x-monitored',
        edit      : '.x-edit',
        refresh   : '.x-refresh',
        rename    : '.x-rename',
        search    : '.x-search'
    },

    events : {
        'click .x-episode-file-editor' : '_openEpisodeFileEditor',
        'click .x-monitored'           : '_toggleMonitored',
        'click .x-edit'                : '_editSeries',
        'click .x-refresh'             : '_refreshSeries',
        'click .x-rename'              : '_renameSeries',
        'click .x-search'              : '_seriesSearch'
    },

    initialize : function() {
        this.seriesCollection = SeriesCollection.clone();
        this.seriesCollection.shadowCollection.bindSignalR();

        this.listenTo(this.model, 'change:monitored', this._setMonitoredState);
        this.listenTo(this.model, 'remove', this._seriesRemoved);
        this.listenTo(vent, vent.Events.CommandComplete, this._commandComplete);

        this.listenTo(this.model, 'change', function(model, options) {
            if (options && options.changeSource === 'signalr') {
                this._refresh();
            }
        });
    },

    onShow : function() {
        $('body').addClass('backdrop');
        var fanArt = this._getFanArt();

        if (fanArt) {
            this._backstrech = $.backstretch(fanArt);
        } else {
            $('body').removeClass('backdrop');
        }

        this._showSeasons();
        this._setMonitoredState();
        this._showInfo();
    },

    onRender : function() {
        CommandController.bindToCommand({
            element : this.ui.refresh,
            command : {
                name : 'refreshSeries'
            }
        });
        CommandController.bindToCommand({
            element : this.ui.search,
            command : {
                name : 'seriesSearch'
            }
        });

        CommandController.bindToCommand({
            element : this.ui.rename,
            command : {
                name         : 'renameFiles',
                seriesId     : this.model.id,
                seasonNumber : -1
            }
        });
    },

    onClose : function() {
        if (this._backstrech) {
            this._backstrech.destroy();
            delete this._backstrech;
        }

        $('body').removeClass('backdrop');
        reqres.removeHandler(reqres.Requests.GetEpisodeFileById);
    },

    _getFanArt : function() {
        var fanArt = _.where(this.model.get('images'), { coverType : 'fanart' });

        if (fanArt && fanArt[0]) {
            return fanArt[0].url;
        }

        return undefined;
    },

    _toggleMonitored : function() {
        var savePromise = this.model.save('monitored', !this.model.get('monitored'), { wait : true });

        this.ui.monitored.spinForPromise(savePromise);
    },

    _setMonitoredState : function() {
        var monitored = this.model.get('monitored');

        this.ui.monitored.removeAttr('data-idle-icon');
        this.ui.monitored.removeClass('fa-spin icon-sonarr-spinner');

        if (monitored) {
            this.ui.monitored.addClass('icon-sonarr-monitored');
            this.ui.monitored.removeClass('icon-sonarr-unmonitored');
            this.$el.removeClass('series-not-monitored');
        } else {
            this.ui.monitored.addClass('icon-sonarr-unmonitored');
            this.ui.monitored.removeClass('icon-sonarr-monitored');
            this.$el.addClass('series-not-monitored');
        }
    },

    _editSeries : function() {
        vent.trigger(vent.Commands.EditSeriesCommand, { series : this.model });
    },

    _refreshSeries : function() {
        CommandController.Execute('refreshSeries', {
            name     : 'refreshSeries',
            seriesId : this.model.id
        });
    },

    _seriesRemoved : function() {
        Backbone.history.navigate('/', { trigger : true });
    },

    _renameSeries : function() {
        vent.trigger(vent.Commands.ShowRenamePreview, { series : this.model });
    },

    _seriesSearch : function() {
        CommandController.Execute('seriesSearch', {
            name     : 'seriesSearch',
            seriesId : this.model.id
        });
    },

    _showSeasons : function() {
        var self = this;

        this.seasons.show(new LoadingView());

        this.seasonCollection = new SeasonCollection(this.model.get('seasons'));
        this.episodeCollection = new EpisodeCollection({ seriesId : this.model.id }).bindSignalR();
        this.episodeFileCollection = new EpisodeFileCollection({ seriesId : this.model.id }).bindSignalR();

        reqres.setHandler(reqres.Requests.GetEpisodeFileById, function(episodeFileId) {
            return self.episodeFileCollection.get(episodeFileId);
        });

        reqres.setHandler(reqres.Requests.GetAlternateNameBySeasonNumber, function(seriesId, seasonNumber) {
            if (self.model.get('id') !== seriesId) {
                return [];
            }

            return _.where(self.model.get('alternateTitles'), { seasonNumber : seasonNumber });
        });

        $.when(this.episodeCollection.fetch(), this.episodeFileCollection.fetch()).done(function() {
            var seasonCollectionView = new SeasonCollectionView({
                collection        : self.seasonCollection,
                episodeCollection : self.episodeCollection,
                series            : self.model
            });

            if (!self.isClosed) {
                self.seasons.show(seasonCollectionView);
            }
        });
    },

    _showInfo : function() {
        this.info.show(new InfoView({
            model                 : this.model,
            episodeFileCollection : this.episodeFileCollection
        }));
    },

    _commandComplete : function(options) {
        if (options.command.get('name') === 'renamefiles') {
            if (options.command.get('seriesId') === this.model.get('id')) {
                this._refresh();
            }
        }
    },

    _refresh : function() {
        this.seasonCollection.add(this.model.get('seasons'), { merge : true });
        this.episodeCollection.fetch();
        this.episodeFileCollection.fetch();

        this._setMonitoredState();
        this._showInfo();
    },

    _openEpisodeFileEditor : function() {
        var view = new EpisodeFileEditorLayout({
            series            : this.model,
            episodeCollection : this.episodeCollection
        });

        vent.trigger(vent.Commands.OpenModalCommand, view);
    }
});