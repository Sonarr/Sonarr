var Handlebars = require('handlebars');
var FormatHelpers = require('../../Shared/FormatHelpers');

Handlebars.registerHelper('Bytes', function(size) {
    return new Handlebars.SafeString(FormatHelpers.bytes(size));
});

Handlebars.registerHelper('Pad2', function(input) {
    return FormatHelpers.pad(input, 2);
});

Handlebars.registerHelper('Number', function(input) {
    return FormatHelpers.number(input);
});
