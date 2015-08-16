var Marionette = require('marionette');
var SummaryLayout = require('./Summary/MovieSummaryLayout');
var SearchLayout = require('./Search/MovieSearchLayout');
var MovieHistoryLayout = require('./History/MovieHistoryLayout');
var Messenger = require('../../Shared/Messenger');

module.exports = Marionette.Layout.extend({
    className : 'modal-lg',
    template  : 'Movie/Modal/MovieDetailsLayoutTemplate',

    regions : {
        summary : '#movie-summary',
        history : '#movie-history',
        search  : '#movie-search'
    },

    ui : {
        summary   : '.x-movie-summary',
        history   : '.x-movie-history',
        search    : '.x-movie-search',
        monitored : '.x-movie-monitored'
    },

    events : {

        'click .x-movie-summary'   : '_showSummary',
        'click .x-movie-history'   : '_showHistory',
        'click .x-movie-search'    : '_showSearch',
        'click .x-movie-monitored' : '_toggleMonitored'
    },

    templateHelpers : {},

    initialize : function(options) {
        this.templateHelpers.hideMovieLink = options.hideMovieLink;

        this.openingTab = options.openingTab || 'summary';
        this.movieFileCollection = options.movieFileCollection;

        this.listenTo(this.model, 'sync', this._setMonitoredState);
    },

    onShow : function() {
        this.searchLayout = new SearchLayout({ model : this.model });

        if (this.openingTab === 'search') {
            this.searchLayout.startManualSearch = true;
            this._showSearch();
        }

        else {
            this._showSummary();
        }

        this._setMonitoredState();
    },

    _showSummary : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.ui.summary.tab('show');
        this.summary.show(new SummaryLayout({
            model               : this.model,
            movieFileCollection : this.movieFileCollection
        }));
    },

    _showHistory : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.ui.history.tab('show');
        this.history.show(new MovieHistoryLayout({
            model  : this.model
        }));
    },

    _showSearch : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.ui.search.tab('show');
        this.search.show(this.searchLayout);
    },

    _toggleMonitored : function() {
        var name = 'monitored';
        this.model.set(name, !this.model.get(name), { silent : true });

        this.ui.monitored.addClass('icon-sonarr-spinner fa-spin');
        this.model.save();
    },

    _setMonitoredState : function() {
        this.ui.monitored.removeClass('fa-spin icon-sonarr-spinner');

        if (this.model.get('monitored')) {
            this.ui.monitored.addClass('icon-sonarr-monitored');
            this.ui.monitored.removeClass('icon-sonarr-unmonitored');
        } else {
            this.ui.monitored.addClass('icon-sonarr-unmonitored');
            this.ui.monitored.removeClass('icon-sonarr-monitored');
        }
    }
});