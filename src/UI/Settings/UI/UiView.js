'use strict';
define(
    [
        'vent',
        'marionette',
        'Shared/UiSettingsModel',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView'
    ], function (vent, Marionette, UiSettingsModel, AsModelBoundView, AsValidatedView) {
        var view = Marionette.ItemView.extend({
            template: 'Settings/UI/UiViewTemplate',

            initialize: function () {
                this.listenTo(this.model, 'sync', this._reloadUiSettings);
            },

            _reloadUiSettings: function() {
                UiSettingsModel.fetch();
            }
        });

        AsModelBoundView.call(view);
        AsValidatedView.call(view);

        return view;
    });

