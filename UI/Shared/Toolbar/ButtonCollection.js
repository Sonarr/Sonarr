'use strict';
define(['app', 'Shared/Toolbar/ButtonModel'], function () {
   return Backbone.Collection.extend({
        model: NzbDrone.Shared.Toolbar.ButtonModel
    });
});

