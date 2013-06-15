"use strict";
define(['app',
        'Shared/Footer/Model'], function () {
    NzbDrone.Shared.Footer.View = Backbone.Marionette.ItemView.extend({

        template: 'Shared/Footer/Template',

        initialize: function () {
            this.model = new NzbDrone.Shared.Footer.Model();
            this.model.set('version', NzbDrone.Constants.Version);
        }
    });

    return new NzbDrone.Shared.Footer.View();
});
