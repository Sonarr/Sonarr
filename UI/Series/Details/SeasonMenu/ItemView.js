'use strict';
define(
    [
        'marionette',
        'Shared/Actioneer'
    ], function (Marionette, Actioneer) {
        return Marionette.ItemView.extend({
            template: 'Series/Details/SeasonMenu/ItemViewTemplate',
            tagName : 'span',

            ui: {
                seasonMonitored: '.x-season-monitored'
            },

            events: {
                'click .x-season-monitored': '_seasonMonitored',
                'click .x-text': '_gotoSeason'
            },

            initialize: function (options) {

                if (!options.episodeCollection) {
                    throw 'episodeCollection is needed';
                }

                this.episodeCollection = options.episodeCollection.bySeason(this.model.get('seasonNumber'));

                var allDownloaded = _.all(this.episodeCollection.models, function (model) {
                    var hasFile = model.get('hasFile');
                    return hasFile;
                });

                this.model.set({
                   allFilesDownloaded: allDownloaded
                });

                this.listenTo(this.model, 'sync', function () {
                    this.render();
                }, this);
            },

            onRender: function () {
                this._setSeasonMonitoredState();
            },

            _seasonSearch: function () {
                Actioneer.ExecuteCommand({
                    command     : 'seasonSearch',
                    properties  : {
                        seriesId    : this.model.get('seriesId'),
                        seasonNumber: this.model.get('seasonNumber')
                    },
                    element     : this.ui.seasonSearch,
                    failMessage : 'Search for season {0} failed'.format(this.model.get('seasonNumber')),
                    startMessage: 'Search for season {0} started'.format(this.model.get('seasonNumber'))
                });
            },

            _seasonMonitored: function (e) {
                e.preventDefault();

                var name = 'monitored';
                this.model.set(name, !this.model.get(name));

                Actioneer.SaveModel({
                    context       : this,
                    element       : this.ui.seasonMonitored
                });
            },

            _setSeasonMonitoredState: function () {
                this.ui.seasonMonitored.removeClass('icon-spinner icon-spin');

                if (this.model.get('monitored')) {
                    this.ui.seasonMonitored.addClass('icon-bookmark');
                    this.ui.seasonMonitored.removeClass('icon-bookmark-empty');
                }
                else {
                    this.ui.seasonMonitored.addClass('icon-bookmark-empty');
                    this.ui.seasonMonitored.removeClass('icon-bookmark');
                }
            },

            _gotoSeason: function () {
                window.location.hash = '#season-' + this.model.get('seasonNumber');
            }
        });
    });
