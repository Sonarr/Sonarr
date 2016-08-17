var Handlebars = require('handlebars');

var metaOptionCurrentConfig = {};

metaOptionCurrentConfig = require('../../../../_output/UI/Content/metaOption.json');


(function _registerMetaHelpers(config) {

    Handlebars.registerHelper('meta-with', function(context, options) {

        if (!options) {
            return '';
        }

        var optName = options.hash.option;

        if (config && (optName in config)) {
            context.metaOption = config[optName];
        } else {
            context.metaOption = undefined;
        }

        return options.fn(context);
    });

    var handlerFactory = function handlerFactory(decideFun, inverseDecision) {
        return function _metaHelperGenerated (context, options) {
            if (!options) {
                return '';
            }

            // ((Boolean(inverseDecision)) !== decideFun(context));

            var dec1 = decideFun(context);
            var dec2 = ((Boolean(inverseDecision)) !== dec1);

            // inverseDecision XOR decideFun
            return dec2 ? options.fn(context) : options.inverse(context);
        };
    };

    // just an understandable constant:
    var INVERSE = 1;

    var decideByFieldName = function decideByFieldName(context, fieldName, defaultValue) {
        if (!context) {
            return defaultValue;
        }
        if (!context.metaOption) {
            return defaultValue;
        }
        if (!(fieldName in context.metaOption)) {
            return defaultValue;
        }
        return Boolean(context.metaOption[fieldName]);
    };

    var decideWrite = function decideWrite(context) {
        return decideByFieldName(context, 'writable', true);
    };

    Handlebars.registerHelper('meta-writable', handlerFactory(decideWrite));
    Handlebars.registerHelper('meta-not-writable', handlerFactory(decideWrite, INVERSE));

    var decideRead = function decideRead(context) {
        return decideByFieldName(context, 'readable', true);
    };

    Handlebars.registerHelper('meta-readable', handlerFactory(decideRead));
    Handlebars.registerHelper('meta-not-readable', handlerFactory(decideRead, INVERSE));

    Handlebars.registerHelper('meta-visible', handlerFactory(decideRead));
    Handlebars.registerHelper('meta-not-visible', handlerFactory(decideRead, INVERSE));

})(metaOptionCurrentConfig);
