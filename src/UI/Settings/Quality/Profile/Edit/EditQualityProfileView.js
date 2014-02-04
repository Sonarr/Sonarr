'use strict';
define(
    [
        'underscore',
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView'
    ], function (_, Marionette, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Quality/Profile/Edit/EditQualityProfileViewTemplate',

            ui: {
                cutoff   : '.x-cutoff'
            },

            getCutoff: function () {
                var self = this;

                return _.findWhere(_.pluck(this.model.get('items'), 'quality'), { id: parseInt(self.ui.cutoff.val(), 10)});
            }
        });

        AsValidatedView.call(view);
        return AsModelBoundView.call(view);
    });
