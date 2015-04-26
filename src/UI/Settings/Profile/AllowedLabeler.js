var Handlebars = require('handlebars');
var _ = require('underscore');

Handlebars.registerHelper('allowedLabeler', function() {
    var ret = '';
    var cutoff = this.cutoff;

    _.each(this.items, function(item) {
        if (item.allowed) {
            if (item.quality.id === cutoff.id) {
                ret += '<li><span class="label label-info" title="Cutoff">' + item.quality.name + '</span></li>';
            } else {
                ret += '<li><span class="label label-default">' + item.quality.name + '</span></li>';
            }
        }
    });

    return new Handlebars.SafeString(ret);
});