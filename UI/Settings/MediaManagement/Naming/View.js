﻿'use strict';
define(
    [
        'marionette',
        'Mixins/AsModelBoundView'
    ], function (Marionette, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/MediaManagement/Naming/ViewTemplate',

            ui: {
                namingOptions        : '.x-naming-options',
                renameEpisodesCheckbox : '.x-rename-episodes'
            },

            events: {
                'change .x-rename-episodes': '_toggleNamingOptions'
            },

            onShow: function () {
                var renameEpisodes = this.model.get('renameEpisodes');
                this._setNamingOptionsVisibility(renameEpisodes);
            },

            _toggleNamingOptions: function() {
                var checked = this.ui.renameEpisodesCheckbox.prop('checked');
                this._setNamingOptionsVisibility(checked);
            },

            _setNamingOptionsVisibility: function (showNamingOptions) {

                if (showNamingOptions) {
                    this.ui.namingOptions.show();
                }

                else {
                    this.ui.namingOptions.hide();
                }
            }
        });

        return AsModelBoundView.call(view);
    });
