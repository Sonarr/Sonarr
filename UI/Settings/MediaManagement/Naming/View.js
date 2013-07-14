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
                renameEpisodesCheckbox: '.x-rename-episodes'
            },

            events: {
                'change .x-rename-episodes': '_setNamingOptionsVisibility'
            },

            onRender: function(){
                if(!this.model.get('renameEpisodes')){
                    this.ui.namingOptions.hide();
                }
            },

            _setNamingOptionsVisibility: function () {
                var checked = this.ui.renameEpisodesCheckbox.prop('checked');
                if (checked) {
                    this.ui.namingOptions.slideDown();
                }

                else {
                    this.ui.namingOptions.slideUp();
                }
            }
        });

        return AsModelBoundView.call(view);
    });
