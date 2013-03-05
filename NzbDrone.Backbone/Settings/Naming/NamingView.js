'use strict';

define([
        'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.Naming.NamingView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Naming/NamingTemplate',
        className: 'form-horizontal',

        ui: {
            switch: '.switch',
            tooltip: '.help-inline i'
        },

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
            this.ui.switch.bootstrapSwitch();
            this.ui.tooltip.tooltip({ placement: 'right' });
        }
    });
});
