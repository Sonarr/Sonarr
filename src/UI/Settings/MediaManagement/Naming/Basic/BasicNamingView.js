'use strict';
define(
    [
        'underscore',
        'vent',
        'marionette',
        'Settings/MediaManagement/Naming/NamingSampleModel',
        'Mixins/AsModelBoundView'
    ], function (_, vent, Marionette, NamingSampleModel, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/MediaManagement/Naming/Basic/BasicNamingViewTemplate',

            ui: {
                namingOptions         : '.x-naming-options',
                singleEpisodeExample  : '.x-single-episode-example',
                multiEpisodeExample   : '.x-multi-episode-example',
                dailyEpisodeExample   : '.x-daily-episode-example'
            },

            onRender: function () {
                this.listenTo(this.model, 'change', this._buildFormat);
                this._buildFormat();
            },

            _updateSamples: function () {
                var data = {
                    renameEpisodes: true,
                    standardEpisodeFormat: this.standardEpisodeFormat,
                    dailyEpisodeFormat: this.dailyEpisodeFormat,
                    multiEpisodeStyle: this.model.get('multiEpisodeStyle')
                };

                this.namingSampleModel.fetch({data: data});
            },

            _buildFormat: function () {
                if (_.has(this.model.changed, 'standardEpisodeFormat') || _.has(this.model.changed, 'dailyEpisodeFormat')) {
                    return;
                }

                var standardEpisodeFormat = '';
                var dailyEpisodeFormat = '';

                if (this.model.get('includeSeriesTitle')) {
                    if (this.model.get('replaceSpaces')) {
                        standardEpisodeFormat += '{Series.Title}';
                        dailyEpisodeFormat += '{Series.Title}';
                    }

                    else {
                        standardEpisodeFormat += '{Series Title}';
                        dailyEpisodeFormat += '{Series Title}';
                    }

                    standardEpisodeFormat += this.model.get('separator');
                    dailyEpisodeFormat += this.model.get('separator');
                }

                standardEpisodeFormat += this.model.get('numberStyle');
                dailyEpisodeFormat += '{Air-Date}';

                if (this.model.get('includeEpisodeTitle')) {
                    standardEpisodeFormat += this.model.get('separator');
                    dailyEpisodeFormat += this.model.get('separator');

                    if (this.model.get('replaceSpaces')) {
                        standardEpisodeFormat += '{Episode.Title}';
                        dailyEpisodeFormat += '{Episode.Title}';
                    }

                    else {
                        standardEpisodeFormat += '{Episode Title}';
                        dailyEpisodeFormat += '{Episode Title}';
                    }
                }

                if (this.model.get('includeQuality')) {
                    if (this.model.get('replaceSpaces')) {
                        standardEpisodeFormat += ' {Quality.Title}';
                        dailyEpisodeFormat += ' {Quality.Title}';
                    }

                    else {
                        standardEpisodeFormat += ' {Quality Title}';
                        dailyEpisodeFormat += ' {Quality Title}';
                    }
                }

                if (this.model.get('replaceSpaces')) {
                    standardEpisodeFormat = standardEpisodeFormat.replace(/\s/g, '.');
                    dailyEpisodeFormat = dailyEpisodeFormat.replace(/\s/g, '.');
                }

                this.model.set('standardEpisodeFormat', standardEpisodeFormat);
                this.model.set('dailyEpisodeFormat', dailyEpisodeFormat);
            }
        });

        return AsModelBoundView.call(view);
    });
