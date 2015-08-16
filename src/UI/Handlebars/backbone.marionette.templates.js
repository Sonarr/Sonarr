var Handlebars = require('handlebars');
require('handlebars.helpers');
require('./Helpers/DateTime');
require('./Helpers/Html');
require('./Helpers/Numbers');
require('./Helpers/Episode');
require('./Helpers/Series');
require('./Helpers/Movies');
require('./Helpers/Quality');
require('./Helpers/System');
require('./Helpers/EachReverse');
require('./Helpers/String');
require('./Handlebars.Debug');

module.exports = function() {
    this.get = function(templateId) {
        var templateKey = templateId.toLowerCase().replace('template', '');

        var templateFunction = window.T[templateKey];

        if (!templateFunction) {
            throw 'couldn\'t find pre-compiled template ' + templateKey;
        }

        return function(data) {
            try {
                var wrappedTemplate = Handlebars.template.call(Handlebars, templateFunction);
                return wrappedTemplate(data);
            }
            catch (error) {
                console.error('template render failed for ' + templateKey + ' ' + error);
                console.error(data);
                throw error;
            }
        };
    };
};