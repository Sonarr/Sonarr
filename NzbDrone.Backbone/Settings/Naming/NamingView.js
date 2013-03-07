'use strict';

define([
        'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.Naming.NamingView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Naming/NamingTemplate',
        className: 'form-horizontal',

        ui: {
            switch: '.switch',
            tooltip: '[class^="help-inline"] i'
        },

        initialize: function(){
            //Listen to save event
            this.bind
        },

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
            this.ui.switch.bootstrapSwitch();
            this.ui.tooltip.tooltip({ placement: 'right' });
        }
    });
});
