'use strict';

define([
    'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.Misc.MiscView = Backbone.Marionette.ItemView.extend({
        template : 'Settings/Misc/MiscTemplate',
        className: 'form-horizontal',

        ui: {
            tooltip: '[class^="help-inline"] i'
        },

        onRender: function () {
            this.ui.tooltip.tooltip({ placement: 'right', html: true });
        }
    });
});
