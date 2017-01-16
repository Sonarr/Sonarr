var vent = require('vent');
var Marionette = require('marionette');
var ButtonsView = require('./ButtonsView');
var ManualSearchLayout = require('./ManualLayout');
var ReleaseCollection = require('../../Release/ReleaseCollection');
var CommandController = require('../../Commands/CommandController');
var LoadingView = require('../../Shared/LoadingView');
var NoResultsView = require('./NoResultsView');

module.exports = Marionette.Layout.extend({
    template : 'Episode/Search/EpisodeSearchLayoutTemplate',

    regions : {
        main : '#episode-search-region'
    },

    events : {
        'click .x-search-auto'   : '_searchAuto',
        'click .x-search-manual' : '_searchManual',
        'click .x-search-back'   : '_showButtons'
    },

    initialize : function() {
        this.mainView = new ButtonsView();
        this.releaseCollection = new ReleaseCollection();

        this.listenTo(this.releaseCollection, 'sync', this._showSearchResults);
    },

    onShow : function() {
        if (this.startManualSearch) {
            this._searchManual();
        }

        else {
            this._showMainView();
        }
    },

    _searchAuto : function(e) {
        if (e) {
            e.preventDefault();
        }

        CommandController.Execute('episodeSearch', {
            episodeIds : [this.model.get('id')]
        });

        vent.trigger(vent.Commands.CloseModalCommand);
    },

    _searchManual : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.mainView = new LoadingView();
        this._showMainView();
        this.releaseCollection.fetchEpisodeReleases(this.model.id);
    },

    _showMainView : function() {
        this.main.show(this.mainView);
    },

    _showButtons : function() {
        this.mainView = new ButtonsView();
        this._showMainView();
    },

    _showSearchResults : function() {
        if (this.releaseCollection.length === 0) {
            this.mainView = new NoResultsView();
        }

        else {
            this.mainView = new ManualSearchLayout({ collection : this.releaseCollection });
        }

        this._showMainView();
    }
});