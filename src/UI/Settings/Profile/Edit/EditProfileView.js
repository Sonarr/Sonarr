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
                cutoff    : '.x-cutoff'
            },

            templateHelpers: function () {
                return {
                    languages : LanguageCollection.toJSON()
                };
            },

            getCutoff: function () {
                var self = this;

                return _.findWhere(_.pluck(this.model.get('items'), 'quality'), { id: parseInt(self.ui.cutoff.val(), 10)});
            }
        });

        AsValidatedView.call(view);
        return AsModelBoundView.call(view);
    });
