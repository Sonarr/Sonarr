'use strict';
define(
    [
        'marionette',
        'Mixins/AsModelBoundView'
    ], function (Marionette, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/MediaManagement/Naming/ViewTemplate',

            ui: {
                namingOptions         : '.x-naming-options',
                renameEpisodesCheckbox: '.x-rename-episodes',
                singleEpisodeExample  : '.x-single-episode-example',
                multiEpisodeExample   : '.x-multi-episode-example'
            },

            events: {
                'change .x-rename-episodes': '_setNamingOptionsVisibility'
            },

            onRender: function () {
                if (!this.model.get('renameEpisodes')) {
                    this.ui.namingOptions.hide();
                }

                this.listenTo(this.model, 'change', this._updateExamples);
                this._updateExamples();
            },

            _setNamingOptionsVisibility: function () {
                var checked = this.ui.renameEpisodesCheckbox.prop('checked');
                if (checked) {
                    this.ui.namingOptions.slideDown();
                }

                else {
                    this.ui.namingOptions.slideUp();
                }
            },

            _updateExamples: function () {
                //TODO: make this use events/listeners
                var self = this;

                var promise = $.ajax({
                    type: 'GET',
                    url : window.NzbDrone.ApiRoot + '/config/naming/samples',
                    data: this.model.toJSON()
                });

                promise.done(function (result) {
                    self.ui.singleEpisodeExample.html(result.singleEpisodeExample);
                    self.ui.multiEpisodeExample.html(result.multiEpisodeExample);
                });
            }
        });

        return AsModelBoundView.call(view);
    });
