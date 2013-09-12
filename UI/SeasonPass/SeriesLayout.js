'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Series/SeasonCollection'
    ], function (Marionette, Backgrid, SeasonCollection) {
        return Marionette.Layout.extend({
            template: 'SeasonPass/SeriesLayoutTemplate',

            ui: {
                seasonSelect: '.x-season-select',
                expander    : '.x-expander',
                seasonGrid  : '.x-season-grid'
            },

            events: {
                'change .x-season-select': '_seasonSelected',
                'click .x-expander'      : '_expand',
                'click .x-latest'        : '_latest',
                'click .x-monitored'     : '_toggleSeasonMonitored'
            },

            regions: {
                seasonGrid: '.x-season-grid'
            },

            initialize: function () {
                this.seasonCollection = new SeasonCollection(this.model.get('seasons'));
                this.expanded = false;
            },

            onRender: function () {
                if (!this.expanded) {
                    this.ui.seasonGrid.hide();
                }

                this._setExpanderIcon();
            },

            _seasonSelected: function () {
                var seasonNumber = parseInt(this.ui.seasonSelect.val());

                if (seasonNumber == -1 || isNaN(seasonNumber)) {
                    return;
                }

                this._setMonitored(seasonNumber)
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

                this._setMonitored(season.seasonNumber);
            },

            _setMonitored: function (seasonNumber) {
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
                    seasonNumber = parseInt($(e.target).parent('td').attr('data-season-number'));
                    element = $(e.target);
                }

                else {
                    seasonNumber = parseInt($(e.target).attr('data-season-number'));
                    element = $(e.target).children('i');
                }

                this.model.setSeasonMonitored(seasonNumber);

                var savePromise =this.model.save()
                    .always(this.render.bind(this));
                element.spinForPromise(savePromise);
            },

            _afterToggleSeasonMonitored: function () {
                this.render();
            }
        });
    });
