 'use strict';

define([
    'vent',
    'AppLayout',
    'marionette',
    'Settings/Profile/Delay/Delete/DelayProfileDeleteView',
    'Mixins/AsModelBoundView',
    'Mixins/AsValidatedView',
    'Mixins/AsEditModalView',
    'Mixins/TagInput',
    'bootstrap'
], function (vent, AppLayout, Marionette, DeleteView, AsModelBoundView, AsValidatedView, AsEditModalView) {

    var view = Marionette.ItemView.extend({
        template: 'Settings/Profile/Delay/Edit/DelayProfileEditViewTemplate',

        _deleteView: DeleteView,

        ui: {
            tags : '.x-tags'
        },

        initialize: function (options) {
            this.targetCollection = options.targetCollection;
        },

        onRender: function () {
            if (this.model.id !== 1) {
                this.ui.tags.tagInput({
                    model    : this.model,
                    property : 'tags'
                });
            }
        },

        _onAfterSave: function () {
            this.targetCollection.add(this.model, { merge: true });
            vent.trigger(vent.Commands.CloseModalCommand);
        }
    });

    AsModelBoundView.call(view);
    AsValidatedView.call(view);
    AsEditModalView.call(view);

    return view;
});
