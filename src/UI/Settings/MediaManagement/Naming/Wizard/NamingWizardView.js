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

            formatsUpdated: 'formatsUpdated',

            initialize: function () {
                this.model = new NamingWizardModel();
                this.namingSampleModel = new NamingSampleModel();
            },

            onRender: function () {
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
                var options = {
                    standardEpisodeFormat: this.standardEpisodeFormat,
                    dailyEpisodeFormat: this.dailyEpisodeFormat,
                    multiEpisodeStyle: this.model.get('multiEpisodeStyle')
                };

                this.trigger(this.formatsUpdated, options);


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

                this.standardEpisodeFormat += this.model.get('numberStyle');
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
