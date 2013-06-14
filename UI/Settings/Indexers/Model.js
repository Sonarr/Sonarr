"use strict";
define(['app',
    'Mixins/SaveIfChangedModel',
    'backbone.deepmodel'], function (App, SaveIfChangedModel, DeepModel) {
    NzbDrone.Settings.Indexers.Model = DeepModel.DeepModel.extend({
    });

    _.extend(NzbDrone.Settings.Indexers.Model.prototype, NzbDrone.Mixins.SaveIfChangedModel);
});
