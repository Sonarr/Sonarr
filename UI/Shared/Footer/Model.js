"use strict";
define(['app'], function () {

    NzbDrone.Shared.Footer.Model = Backbone.Model.extend({
        defaults: {
            'version'   : '0.0.0.0',
            'buildDate' : Date.create()
        },

        mutators: {
            humanizedBuildDate: function () {
                var date = Date.create(this.get('buildDate'));

                return date.short();
            }
        }
    });

    return NzbDrone.Shared.Footer.Model;
});
