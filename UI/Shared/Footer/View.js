"use strict";
define(['app',
        'Shared/Footer/Model'], function (App, FooterModel) {
    return Backbone.Marionette.ItemView.extend({

        template: 'Shared/Footer/Template',

        initialize: function () {
            this.model = new FooterModel();
            this.model.set('version', NzbDrone.Constants.Version);
            this.model.set('buildDate', NzbDrone.Constants.BuildDate);
        }
    });
});
