"use strict";
define(['app',
        'Mixins/SaveIfChangedModel'], function () {
    NzbDrone.Settings.Naming.NamingModel = Backbone.Model.extend({
        url: NzbDrone.Constants.ApiRoot + '/config/naming'
    });

    _.extend(NzbDrone.Settings.Naming.NamingModel.prototype, NzbDrone.Mixins.SaveIfChangedModel);
});
