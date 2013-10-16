'use strict';
define(
    [
        'underscore',
        'marionette',
        'backgrid',
        'Series/SeasonCollection'
    ], function (_, Marionette, Backgrid, SeasonCollection) {
        return Marionette.Layout.extend({
            template: 'SeasonPass/SeriesLayoutTemplate',

            ui: {
                seasonSelect   : '.x-season-select',
                expander       : '.x-expander',
                seasonGrid     : '.x-season-grid',
                seriesMonitored: '.x-series-monitored'
            },

            events: {
                'change .x-season-select'  : '_seasonSelected',
                'click .x-expander'        : '_expand',
                'click .x-latest'          : '_latest',
                'click .x-all'             : '_all',
                'click .x-monitored'       : '_toggleSeasonMonitored',
                'click .x-series-monitored': '_toggleSeriesMonitored'
            },

            regions: {
                seasonGrid: '.x-season-grid'
            },

            initialize: function () {
                this.listenTo(this.model, 'sync', this._setSeriesMonitoredState);
                this.seasonCollection = new SeasonCollection(this.model.get('seasons'));
                this.expanded = false;
            },

            onRender: function () {
                if (!this.expanded) {
                    this.ui.seasonGrid.hide();
                }

                this._setExpanderIcon();
                this._setSeriesMonitoredState();
            },

            _seasonSelected: function () {
                var seasonNumber = parseInt(this.ui.seasonSelect.val(), 10);

                if (seasonNumber === -1 || isNaN(seasonNumber)) {
                    return;
                }

                this._setSeasonMonitored(seasonNumber);
            },

            _expand: function () {
                if (this.expanded) {
                    this.ui.seasonGrid.slideUp();
                    this.expanded = false;
                }

                else {
                    this.ui.seasonGrid.slideDown();
                    this.expanded = true;
                }

                this._setExpanderIcon();
            },

            _setExpanderIcon: function () {
                if (this.expanded) {
                    this.ui.expander.removeClass('icon-chevron-right');
                    this.ui.expander.addClass('icon-chevron-down');
                }

                else {
                    this.ui.expander.removeClass('icon-chevron-down');
                    this.ui.expander.addClass('icon-chevron-right');
                }
            },

            _latest: function () {
                var season = _.max(this.model.get('seasons'), function (s) {
                    return s.seasonNumber;
                });

                this._setSeasonMonitored(season.seasonNumber);
            },

            _all: function () {
                var minSeasonNotZero = _.min(_.reject(this.model.get('seasons'), { seasonNumber: 0 }), 'seasonNumber');

                this._setSeasonMonitored(minSeasonNotZero.seasonNumber);
            },

            _setSeasonMonitored: function (seasonNumber) {
                var self = this;

                this.model.setSeasonPass(seasonNumber);

                var promise = this.model.save();

                promise.done(function (data) {
                    self.seasonCollection = new SeasonCollection(data);
                    self.render();
                });
            },

            _toggleSeasonMonitored: function (e) {
                var seasonNumber = 0;
                var element;

                if (e.target.localName === 'i') {
                    seasonNumber = parseInt($(e.target).parent('td').attr('data-season-number'), 10);
                    element = $(e.target);
                }

                else {
                    seasonNumber = parseInt($(e.target).attr('data-season-number'), 10);
                    element = $(e.target).children('i');
                }

                this.model.setSeasonMonitored(seasonNumber);

                var savePromise = this.model.save().always(this.render.bind(this));
                element.spinForPromise(savePromise);
            },

            _afterToggleSeasonMonitored: function () {
                this.render();
            },

            _setSeriesMonitoredState: function () {
                var monitored = this.model.get('monitored');

                this.ui.seriesMonitored.removeAttr('data-idle-icon');

                if (monitored) {
                    this.ui.seriesMonitored.addClass('icon-nd-monitored');
                    this.ui.seriesMonitored.removeClass('icon-nd-unmonitored');
                }
                else {
                    this.ui.seriesMonitored.addClass('icon-nd-unmonitored');
                    this.ui.seriesMonitored.removeClass('icon-nd-monitored');
                }
            },

            _toggleSeriesMonitored: function (e) {
                var savePromise = this.model.save('monitored', !this.model.get('monitored'), {
                    wait: true
                });

                this.ui.seriesMonitored.spinForPromise(savePromise);
            }
        });
    });
