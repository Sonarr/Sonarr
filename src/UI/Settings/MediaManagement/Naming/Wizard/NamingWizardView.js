'use strict';
define(
    [
        'vent',
        'marionette',
        'Settings/MediaManagement/Naming/NamingSampleModel',
        'Settings/MediaManagement/Naming/Wizard/NamingWizardModel',
        'Mixins/AsModelBoundView'
    ], function (vent, Marionette, NamingSampleModel, NamingWizardModel, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/MediaManagement/Naming/Wizard/NamingWizardViewTemplate',

            ui: {
                namingOptions         : '.x-naming-options',
                singleEpisodeExample  : '.x-single-episode-example',
                multiEpisodeExample   : '.x-multi-episode-example',
                dailyEpisodeExample   : '.x-daily-episode-example'
            },

            events: {
                'click .x-apply': '_applyNaming'
            },

            initialize: function (options) {
                this.model = new NamingWizardModel();
                this.namingModel = options.model;
                this.namingSampleModel = new NamingSampleModel();
            },

            onRender: function () {
                if (!this.model.get('renameEpisodes')) {
                    this.ui.namingOptions.hide();
                }

                this.listenTo(this.model, 'change', this._buildFormat);
                this.listenTo(this.namingSampleModel, 'sync', this._showSamples);
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

            _showSamples: function () {
                this.ui.singleEpisodeExample.html(this.namingSampleModel.get('singleEpisodeExample'));
                this.ui.multiEpisodeExample.html(this.namingSampleModel.get('multiEpisodeExample'));
                this.ui.dailyEpisodeExample.html(this.namingSampleModel.get('dailyEpisodeExample'));
            },

            _applyNaming: function () {
                this.namingModel.set('standardEpisodeFormat', this.standardEpisodeFormat);
                this.namingModel.set('dailyEpisodeFormat', this.dailyEpisodeFormat);
                this.namingModel.set('multiEpisodeStyle', this.model.get('multiEpisodeStyle'));

                vent.trigger(vent.Commands.CloseModalCommand);
            },

            _buildFormat: function () {
                this.standardEpisodeFormat = '';
                this.dailyEpisodeFormat = '';

                if (this.model.get('includeSeriesTitle')) {
                    if (this.model.get('replaceSpaces')) {
                        this.standardEpisodeFormat += '{Series.Title}';
                        this.dailyEpisodeFormat += '{Series.Title}';
                    }

                    else {
                        this.standardEpisodeFormat += '{Series Title}';
                        this.dailyEpisodeFormat += '{Series Title}';
                    }

                    this.standardEpisodeFormat += this.model.get('separator');
                    this.dailyEpisodeFormat += this.model.get('separator');
                }

                switch (this.model.get('numberStyle')) {
                    case '0':
                        this.standardEpisodeFormat += '{season}x{0episode}';
                        break;
                    case '1':
                        this.standardEpisodeFormat += '{0season}x{0episode}';
                        break;
                    case '2':
                        this.standardEpisodeFormat += 'S{0season}E{0episode}';
                        break;
                    case '3':
                        this.standardEpisodeFormat += 's{0season}e{0episode}';
                        break;
                    default:
                        this.standardEpisodeFormat += 'Unknown Number Pattern';
                }

                this.dailyEpisodeFormat += '{Air-Date}';

                if (this.model.get('includeEpisodeTitle')) {
                    this.standardEpisodeFormat += this.model.get('separator');
                    this.dailyEpisodeFormat += this.model.get('separator');

                    if (this.model.get('replaceSpaces')) {
                        this.standardEpisodeFormat += '{Episode.Title}';
                        this.dailyEpisodeFormat += '{Episode.Title}';
                    }

                    else {
                        this.standardEpisodeFormat += '{Episode Title}';
                        this.dailyEpisodeFormat += '{Episode Title}';
                    }
                }

                if (this.model.get('includeQuality')) {
                    if (this.model.get('replaceSpaces')) {
                        this.standardEpisodeFormat += ' {Quality.Title}';
                        this.dailyEpisodeFormat += ' {Quality.Title}';
                    }

                    else {
                        this.standardEpisodeFormat += ' {Quality Title}';
                        this.dailyEpisodeFormat += ' {Quality Title}';
                    }
                }

                if (this.model.get('replaceSpaces')) {
                    this.standardEpisodeFormat = this.standardEpisodeFormat.replace(/\s/g, '.');
                    this.dailyEpisodeFormat = this.dailyEpisodeFormat.replace(/\s/g, '.');
                }

                this._updateSamples();
            }
        });

        return AsModelBoundView.call(view);
    });
