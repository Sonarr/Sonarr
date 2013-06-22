'use strict';
define(['app'], function () {
    NzbDrone.Shared.NotFoundView = Backbone.Marionette.ItemView.extend({
        template: 'Shared/notfoundtemplate'
    });
});


