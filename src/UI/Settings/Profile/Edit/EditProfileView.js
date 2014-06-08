'use strict';
define(
    [
        'underscore',
        'marionette',
        'Settings/Profile/Language/LanguageCollection',
        'Config',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView'
    ], function (_, Marionette, LanguageCollection, Config, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Profile/Edit/EditProfileViewTemplate',

            ui: {
                cutoff    : '.x-cutoff',
                delay     : '.x-delay',
                delayMode : '.x-delay-mode'
            },

            events: {
                'change .x-delay': 'toggleDelayMode',
                'keyup .x-delay': 'toggleDelayMode'
            },

            templateHelpers: function () {
                return {
                    languages : LanguageCollection.toJSON()
                };
            },

            onShow: function () {
                this.toggleDelayMode();
            },

            getCutoff: function () {
                var self = this;

                return _.findWhere(_.pluck(this.model.get('items'), 'quality'), { id: parseInt(self.ui.cutoff.val(), 10)});
            },

            toggleDelayMode: function () {
                var delay = parseInt(this.ui.delay.val(), 10);

                if (isNaN(delay)) {
                    return;
                }

                if (delay > 0 && Config.getValueBoolean(Config.Keys.AdvancedSettings)) {
                    this.ui.delayMode.show();
                }

                else {
                    this.ui.delayMode.hide();
                }
            }
        });

        AsValidatedView.call(view);
        return AsModelBoundView.call(view);
    });
