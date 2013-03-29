'use strict';

define([
    'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.Misc.MiscView = Backbone.Marionette.ItemView.extend({
        template : 'Settings/Misc/MiscTemplate',
        className: 'form-horizontal',

        ui: {
            switch : '.switch',
            tooltip: '[class^="help-inline"] i'
        },

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
            this.ui.switch.bootstrapSwitch();
            this.ui.tooltip.tooltip({ placement: 'right', html: true });
        }
    });
});
