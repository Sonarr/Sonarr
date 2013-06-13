"use strict";
define(['app',
        'Mixins/SaveIfChangedModel'], function () {
    NzbDrone.Settings.SettingsModel = Backbone.Model.extend({
        url: NzbDrone.Constants.ApiRoot + '/settings'
    });

    _.extend(NzbDrone.Settings.SettingsModel.prototype, NzbDrone.Mixins.SaveIfChangedModel);
});
