"use strict";
define(['app', 'Shared/Toolbar/CommandModel'], function () {
    NzbDrone.Shared.Toolbar.CommandCollection = Backbone.Collection.extend({
        model: NzbDrone.Shared.Toolbar.CommandModel
    });
});

