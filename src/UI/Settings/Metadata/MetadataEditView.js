'use strict';

define(
    [
        'vent',
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView'
    ], function (vent, Marionette, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Metadata/MetadataEditViewTemplate',

            ui: {
                activity: '.x-activity'
            },

            events: {
                'click .x-save'        : '_save'
            },

            _save: function () {
                this.ui.activity.html('<i class="icon-nd-spinner"></i>');

                var self = this;
                var promise = this.model.save();

                if (promise) {
                    promise.done(function () {
                        vent.trigger(vent.Commands.CloseModalCommand);
                    });

                    promise.fail(function () {
                        self.ui.activity.empty();
                    });
                }
            }
        });

        AsModelBoundView.call(view);
        AsValidatedView.call(view);

        return view;
    });
