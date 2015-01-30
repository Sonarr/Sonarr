'use strict';

define(
    [
        'vent',
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView',
        'Mixins/AsEditModalView'
    ], function (vent, Marionette, AsModelBoundView, AsValidatedView, AsEditModalView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Metadata/MetadataEditViewTemplate',

            _onAfterSave: function () {
                vent.trigger(vent.Commands.CloseModalCommand);
            }
        });

        AsModelBoundView.call(view);
        AsValidatedView.call(view);
        AsEditModalView.call(view);

        return view;
    });
