'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Series/SeasonCollection',
        'Cells/ToggleCell',
        'Shared/Actioneer'
    ], function (Marionette, Backgrid, SeasonCollection, ToggleCell, Actioneer) {
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
                'click .x-latest'        : '_latest'
            },

            regions: {
                seasonGrid: '.x-season-grid'
            },

            columns:
                [
                    {
                        name      : 'monitored',
                        label     : '',
                        cell      : ToggleCell,
                        trueClass : 'icon-bookmark',
                        falseClass: 'icon-bookmark-empty',
                        tooltip   : 'Toggle monitored status',
                        sortable  : false
                    },
                    {
                        name : 'seasonNumber',
                        label: 'Season',
                        cell : Backgrid.IntegerCell.extend({
                            className: 'season-number-cell'
                        })
                    }
                ],

            initialize: function (options) {
                this.seasonCollection = options.seasonCollection.bySeries(this.model.get('id'));
                this.model.set('seasons', this.seasonCollection);
                this.expanded = false;
            },

            onRender: function () {
                this.seasonGrid.show(new Backgrid.Grid({
                    columns   : this.columns,
                    collection: this.seasonCollection,
                    className : 'table table-condensed season-grid span5'
                }));

                if (!this.expanded) {
                    this.seasonGrid.$el.hide();
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
                var season = _.max(this.seasonCollection.models, function (model) {
                    return model.get('seasonNumber');
                });

                //var seasonNumber = season.get('seasonNumber');

                this._setMonitored(season.get('seasonNumber'))
            },

            _setMonitored: function (seasonNumber) {
                var self = this;

                var promise = $.ajax({
                    url: this.seasonCollection.url + '/pass',
                    type: 'POST',
                    data: {
                        seriesId: this.model.get('id'),
                        seasonNumber: seasonNumber
                    }
                });

                promise.done(function (data) {
                    self.seasonCollection = new SeasonCollection(data);
                    self.render();
                });
            }
        });
    });
