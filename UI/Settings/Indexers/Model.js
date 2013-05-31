"use strict";
define(['app', 'Mixins/SaveIfChangedModel'], function () {
    NzbDrone.Settings.Indexers.Model = Backbone.DeepModel.extend({
    });

    _.extend(NzbDrone.Settings.Indexers.Model.prototype, NzbDrone.Mixins.SaveIfChangedModel);
});
