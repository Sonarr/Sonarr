"use strict";
define(['app'], function () {
    NzbDrone.Settings.Naming.NamingModel = Backbone.Model.extend({
        url: NzbDrone.Constants.ApiRoot + '/config/naming'
    });
});
