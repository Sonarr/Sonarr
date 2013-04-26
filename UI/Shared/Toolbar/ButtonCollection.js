"use strict";
define(['app', 'Shared/Toolbar/ButtonModel'], function () {
    NzbDrone.Shared.Toolbar.ButtonCollection = Backbone.Collection.extend({
        model: NzbDrone.Shared.Toolbar.ButtonModel
    });
});

