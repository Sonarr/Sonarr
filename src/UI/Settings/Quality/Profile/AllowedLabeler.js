'use strict';
define(
    [
        'handlebars',
        'underscore'
    ], function (Handlebars, _) {
        Handlebars.registerHelper('allowedLabeler', function () {
            var ret = '';
            var cutoff = this.cutoff;
            _.each(this.allowed, function (allowed) {
                if (allowed.id === cutoff.id) {
                    ret += '<span class="label label-info" title="Cutoff">' + allowed.name + '</span> ';
                }

                else {
                    ret += '<span class="label">' + allowed.name + '</span> ';
                }
            });

            return new Handlebars.SafeString(ret);
        });
    });
