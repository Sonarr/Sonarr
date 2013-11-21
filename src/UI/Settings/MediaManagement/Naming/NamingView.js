'use strict';
define(
    [
        'vent',
        'marionette',
        'Settings/MediaManagement/Naming/NamingSampleModel',
        'Settings/MediaManagement/Naming/Wizard/NamingWizardView',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView'
    ], function (vent, Marionette, NamingSampleModel, NamingWizardView, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/MediaManagement/Naming/NamingViewTemplate',

            ui: {
                namingOptions         : '.x-naming-options',
                renameEpisodesCheckbox: '.x-rename-episodes',
                singleEpisodeExample  : '.x-single-episode-example',
                multiEpisodeExample   : '.x-multi-episode-example',
                dailyEpisodeExample   : '.x-daily-episode-example',
                namingTokenHelper     : '.x-naming-token-helper'
            },

            events: {
                'change .x-rename-episodes'      : '_setFailedDownloadOptionsVisibility',
                'click .x-show-wizard'           : '_showWizard',
                'click .x-naming-token-helper a' : '_addToken'
            },

            onRender: function () {
                if (!this.model.has('renameEpisodes')) {
                    this.ui.namingOptions.hide();
                }

                this.namingSampleModel = new NamingSampleModel();

                this.listenTo(this.model, 'change', this._updateSamples);
                this.listenTo(this.namingSampleModel, 'sync', this._showSamples);
                this._updateSamples();
            },

            _setFailedDownloadOptionsVisibility: function () {
                var checked = this.ui.renameEpisodesCheckbox.prop('checked');
                if (checked) {
                    this.ui.namingOptions.slideDown();
                }

                else {
                    this.ui.namingOptions.slideUp();
                }
            },

            _updateSamples: function () {
                this.namingSampleModel.fetch({ data: this.model.toJSON() });
            },

            _showSamples: function () {
                this.ui.singleEpisodeExample.html(this.namingSampleModel.get('singleEpisodeExample'));
                this.ui.multiEpisodeExample.html(this.namingSampleModel.get('multiEpisodeExample'));
                this.ui.dailyEpisodeExample.html(this.namingSampleModel.get('dailyEpisodeExample'));
            },

            _showWizard: function () {
                var modalView = new NamingWizardView();
                vent.trigger(vent.Commands.OpenModalCommand, modalView);
                this.listenTo(modalView, modalView.formatsUpdated, this._updateFormats);

                vent.trigger(vent.Commands.ShowNamingWizard, { model: this.model });
            },

            _addToken: function (e) {
                e.preventDefault();
                e.stopPropagation();

                var target = e.target;
                var token = '';
                var input = this.$(target).closest('.x-helper-input').children('input');

                if (this.$(target).attr('data-token')) {
                    token = '{{0}}'.format(this.$(target).attr('data-token'));
                }

                else {
                    token = this.$(target).attr('data-separator');
                }

                input.val(input.val() + token);

                this.ui.namingTokenHelper.removeClass('open');
                input.focus();
            },

            _updateFormats: function (options) {
                this.model.set('standardEpisodeFormat', options.standardEpisodeFormat);
                this.model.set('dailyEpisodeFormat', options.dailyEpisodeFormat);
                this.model.set('multiEpisodeStyle', options.multiEpisodeStyle);
            }
        });

        AsModelBoundView.call(view);
        AsValidatedView.call(view);

        return view;
    });
