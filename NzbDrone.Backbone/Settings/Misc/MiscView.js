'use strict';

define([
        'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.Misc.MiscView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Misc/MiscTemplate',

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        }
    });
});
