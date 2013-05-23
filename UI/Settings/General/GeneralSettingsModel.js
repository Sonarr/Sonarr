"use strict";
define(['app'], function () {
    NzbDrone.Settings.General.GeneralSettingsModel = Backbone.Model.extend({
        url: NzbDrone.Constants.ApiRoot + '/settings/host',

        initialize: function () {
            this.on('change', function () {
                this.isSaved = false;
            }, this);

            this.on('sync', function () {
                this.isSaved = true;
            }, this);
        }
    });
});
