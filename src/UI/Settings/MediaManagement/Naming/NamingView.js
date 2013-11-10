'use strict';
define(
    [
        'jquery',
        'vent',
        'marionette',
        'Settings/MediaManagement/Naming/NamingSampleModel',
        'Mixins/AsModelBoundView'
    ], function ($, vent, Marionette, NamingSampleModel, AsModelBoundView) {

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
                if (!this.model.get('renameEpisodes')) {
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
                vent.trigger(vent.Commands.ShowNamingWizard, { model: this.model });
            },

            _addToken: function (e) {
                e.preventDefault();
                e.stopPropagation();

                var target = e.target;
                var token = '';
                var input = $(target).closest('.x-helper-input').children('input');

                if ($(target).attr('data-token')) {
                    token = '{{0}}'.format($(target).attr('data-token'));
                }

                else {
                    token = $(target).attr('data-separator');
                }

                input.val(input.val() + token);

                this.ui.namingTokenHelper.removeClass('open');
                input.focus();
            }
        });

        return AsModelBoundView.call(view);
    });
