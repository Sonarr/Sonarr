var $ = require('jquery');
var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var Backbone = require('backbone');
var MovieCollection = require('../MovieCollection');
var MovieFileCollection = require('../MovieFileCollection');
var InfoView = require('./InfoView');
var CommandController = require('../../Commands/CommandController');
var LoadingView = require('../../Shared/LoadingView');
var NotFoundView = require('../../Shared/NotFoundView');
var MovieFileEditorLayout = require('../../MovieFile/Editor/MovieFileEditorLayout');
require('backstrech');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template          : 'Movie/Details/MovieDetailsTemplate',

    regions : {
        info      : '#info',
		notfound  : '#not-found'
    },

    ui : {
        header    : '.x-header',
        monitored : '.x-monitored',
        edit      : '.x-edit',
        refresh   : '.x-refresh',
        rename    : '.x-rename',
        search    : '.x-search',
        poster    : '.x-movie-poster'
    },

    events : {
        'click .x-movie-file-editor'   : '_openMovieFileEditor',
        'click .x-monitored'           : '_toggleMonitored',
        'click .x-edit'                : '_editMovie',
        'click .x-refresh'             : '_refreshMovie',
        'click .x-rename'              : '_renameMovie',
        'click .x-search'              : '_movieSearch',
        'click .x-search-manual'       : '_movieManualSearch'
    },

    initialize : function() {
        this.movieCollection = MovieCollection.clone();
        this.movieCollection.shadowCollection.bindSignalR();

        this.listenTo(this.model, 'change:monitored', this._setMonitoredState);
        this.listenTo(this.model, 'remove', this._movieRemoved);
        this.listenTo(vent, vent.Events.CommandComplete, this._commandComplete);

        this.listenTo(this.model, 'change', function(model, options) {
            if (options && options.changeSource === 'signalr') {
                this._refresh();
            }
        });

        this.listenTo(this.model,  'change:images', this._updateImages);
        this.movieFileCollection = new MovieFileCollection({ movieId : this.model.id }).bindSignalR();
        this.movieFileCollection.fetch();
    },

    onShow : function() {	
        this._showBackdrop();
        this._setMonitoredState();
        this._showInfo();
    },

    onRender : function() {
        CommandController.bindToCommand({
            element : this.ui.refresh,
            command : {
                name : 'refreshMovie'
            }
        });

        CommandController.bindToCommand({
            element : this.ui.search,
            command : {
                name : 'movieSearch'
            }
        });

        CommandController.bindToCommand({
            element : this.ui.rename,
            command : {
                name        : 'renameMovieFiles',
                movieId     : this.model.id
            }
        });
    },

    onClose : function() {
        if (this._backstrech) {
            this._backstrech.destroy();
            delete this._backstrech;
        }

        $('body').removeClass('backdrop');
    },

    _getImage : function(type) {
        var image = _.where(this.model.get('images'), { coverType : type });

        if (image && image[0]) {
            return image[0].url;
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

    _editMovie : function() {
        vent.trigger(vent.Commands.EditMovieCommand, { movie : this.model });
    },

    _refreshMovie : function() {
        CommandController.Execute('refreshMovie', {
            name     : 'refreshMovie',
            movieId  : this.model.id
        });
    },

    _movieRemoved : function() {
        Backbone.history.navigate('/movie', { trigger : true });
    },

    _renameMovie : function() {
        vent.trigger(vent.Commands.ShowRenameMoviePreview, { movie : this.model });
    },

    _movieSearch : function() {
        CommandController.Execute('movieSearch', {
            name     : 'movieSearch',
            movieId  : this.model.id
        });
    },

    _movieManualSearch : function(e) {
        vent.trigger(vent.Commands.ShowMovieDetails, {
            movie               : this.model,
            movieFileCollection : this.movieFileCollection,
            hideMovieLink       : true,
            openingTab          : 'search'
        });
    },

     _showInfo : function() {
        this.info.show(new InfoView({
            model                 : this.model,
            movieFileCollection   : this.movieFileCollection
        }));
    },
	
    _showNotFound : function() {
        this.notfound.show(new NotFoundView());
    },
	

    _commandComplete : function(options) {
        if (options.command.get('name') === 'renamemoviefiles') {
            if (options.command.get('movieId') === this.model.get('id')) {
                this._refresh();
            }
        }
    },

    _refresh : function() {
        this.movieFileCollection.fetch();

        this._setMonitoredState();
        this._showInfo();
    },

    _openMovieFileEditor : function() {
        var view = new MovieFileEditorLayout({
            movie               : this.model,
            movieFileCollection : this.movieFileCollection
        });

        vent.trigger(vent.Commands.OpenModalCommand, view);
    },

    _updateImages : function () {
        var poster = this._getImage('poster');

        if (poster) {
            this.ui.poster.attr('src', poster);
        }

        this._showBackdrop();
    },

    _showBackdrop : function () {
        $('body').addClass('backdrop');
        var fanArt = this._getImage('fanart');

        if (fanArt) {
            this._backstrech = $.backstretch(fanArt);
        } else {
            $('body').removeClass('backdrop');
        }
    }
});