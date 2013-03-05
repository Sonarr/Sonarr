'use strict';

define([
        'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.System.SystemView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/System/SystemTemplate',

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        }
    });
});
