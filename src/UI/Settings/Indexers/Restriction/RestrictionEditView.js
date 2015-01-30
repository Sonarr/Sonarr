'use strict';

define([
    'underscore',
    'vent',
    'AppLayout',
    'marionette',
    'Settings/Indexers/Restriction/RestrictionDeleteView',
    'Commands/CommandController',
    'Mixins/AsModelBoundView',
    'Mixins/AsValidatedView',
    'Mixins/AsEditModalView',
    'Mixins/TagInput',
    'bootstrap',
    'bootstrap.tagsinput'
], function (_, vent, AppLayout, Marionette, DeleteView, CommandController, AsModelBoundView, AsValidatedView, AsEditModalView) {

    var view = Marionette.ItemView.extend({
        template  : 'Settings/Indexers/Restriction/RestrictionEditViewTemplate',

        ui : {
            required : '.x-required',
            ignored  : '.x-ignored',
            tags     : '.x-tags'
        },

        _deleteView: DeleteView,

        initialize : function (options) {
            this.targetCollection = options.targetCollection;
        },

        onRender : function () {
            this.ui.required.tagsinput({
                trimValue : true,
                tagClass  : 'label label-success'
            });

            this.ui.ignored.tagsinput({
                trimValue : true,
                tagClass  : 'label label-danger'
            });

            this.ui.tags.tagInput({
                model    : this.model,
                property : 'tags'
            });
        },

        _onAfterSave : function () {
            this.targetCollection.add(this.model, { merge : true });
            vent.trigger(vent.Commands.CloseModalCommand);
        }
    });

    AsModelBoundView.call(view);
    AsValidatedView.call(view);
    AsEditModalView.call(view);

    return view;
});
