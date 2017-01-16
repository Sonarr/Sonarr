var Marionette = require('marionette');
var SummaryLayout = require('./Summary/EpisodeSummaryLayout');
var SearchLayout = require('./Search/EpisodeSearchLayout');
var EpisodeHistoryLayout = require('./History/EpisodeHistoryLayout');
var SeriesCollection = require('../Series/SeriesCollection');
var Messenger = require('../Shared/Messenger');

module.exports = Marionette.Layout.extend({
    className : 'modal-lg',
    template  : 'Episode/EpisodeDetailsLayoutTemplate',

    regions : {
        summary : '#episode-summary',
        history : '#episode-history',
        search  : '#episode-search'
    },

    ui : {
        summary   : '.x-episode-summary',
        history   : '.x-episode-history',
        search    : '.x-episode-search',
        monitored : '.x-episode-monitored'
    },

    events : {

        'click .x-episode-summary'   : '_showSummary',
        'click .x-episode-history'   : '_showHistory',
        'click .x-episode-search'    : '_showSearch',
        'click .x-episode-monitored' : '_toggleMonitored'
    },

    templateHelpers : {},

    initialize : function(options) {
        this.templateHelpers.hideSeriesLink = options.hideSeriesLink;

        this.series = SeriesCollection.get(this.model.get('seriesId'));
        this.templateHelpers.series = this.series.toJSON();
        this.openingTab = options.openingTab || 'summary';

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

        if (this.series.get('monitored')) {
            this.$el.removeClass('series-not-monitored');
        }

        else {
            this.$el.addClass('series-not-monitored');
        }
    },

    _showSummary : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.ui.summary.tab('show');
        this.summary.show(new SummaryLayout({
            model  : this.model,
            series : this.series
        }));
    },

    _showHistory : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.ui.history.tab('show');
        this.history.show(new EpisodeHistoryLayout({
            model  : this.model,
            series : this.series
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
        if (!this.series.get('monitored')) {

            Messenger.show({
                message : 'Unable to change monitored state when series is not monitored',
                type    : 'error'
            });

            return;
        }

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