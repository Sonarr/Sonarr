'use strict';

define([
        'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.DownloadClient.DownloadClientView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/DownloadClient/DownloadClientTemplate',

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        }
    });
});
