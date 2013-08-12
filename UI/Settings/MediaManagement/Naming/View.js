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

            onRender: function(){
                if(!this.model.get('renameEpisodes')){
                    this.ui.namingOptions.hide();
                }

                this.listenTo(this.model, 'change', this._buildExamples);
                this._buildExamples();
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

            _buildExamples: function () {
                var self = this;

                var data = this.model.toJSON();
                data.id = 0;

                var promise = $.ajax({
                    type: 'POST',
                    url : window.ApiRoot + '/naming',
                    data: JSON.stringify(data)
                });

                promise.done(function (result) {
                    self.ui.singleEpisodeExample.html(result.singleEpisodeExample);
                    self.ui.multiEpisodeExample.html(result.multiEpisodeExample);
                });
            }
        });

        return AsModelBoundView.call(view);
    });
