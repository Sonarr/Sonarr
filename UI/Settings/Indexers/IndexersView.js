'use strict';

define([
    'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.Indexers.IndexersView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Indexers/IndexersTemplate'
    });
});
