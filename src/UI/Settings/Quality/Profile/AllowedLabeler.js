'use strict';
define(
    [
        'handlebars',
        'underscore'
    ], function (Handlebars, _) {
        Handlebars.registerHelper('allowedLabeler', function () {
            var ret = '';
            var cutoff = this.cutoff;
            _.each(this.items, function (item) {
                if (item.allowed) {
                    if (item.quality.id === cutoff.id) {
                        ret += '<span class="label label-info" title="Cutoff">' + item.quality.name + '</span>&nbsp;';
                    }
                    else {
                        ret += '<span class="label">' + item.quality.name + '</span>&nbsp;';
                    }
                }
            });

            return new Handlebars.SafeString(ret);
        });
    });
