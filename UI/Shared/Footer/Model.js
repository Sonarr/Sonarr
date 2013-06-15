"use strict";
define(['app'], function () {

    NzbDrone.Shared.Footer.Model = Backbone.Model.extend({
        defaults: {
            'version' : '0.0.0.0'
        }
    });

    return NzbDrone.Shared.Footer.Model;
});
