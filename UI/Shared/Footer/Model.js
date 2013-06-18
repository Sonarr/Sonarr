"use strict";
define(['app'], function () {

    return Backbone.Model.extend({
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
});
