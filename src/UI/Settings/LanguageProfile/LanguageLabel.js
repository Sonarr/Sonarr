var _ = require('underscore');
var Handlebars = require('handlebars');

Handlebars.registerHelper('languageLabel', function() {

    var result = '';
    var cutoff = this.cutoff;
    _.each(this.languages, function (language) {
        if (language.allowed)
        {
            if (language.language.id === cutoff.id) {
                result = '<li><span class="label label-primary" title="Cutoff">' + language.language.name + '</span></li>' + result;
            } else {
                result += '<li><span class="label label-default">' + language.language.name + '</span></li>';
            }
        }
    });

    return new Handlebars.SafeString(result);
});