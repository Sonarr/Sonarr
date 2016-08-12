var Handlebars = require('handlebars');
var MetaOptionConfig = require('../../MetaOptionConfig');

var metaOptionCurrentConfig = {};

metaOptionCurrentConfig = require('../../../../_output/UI/Content/metaOption.json');

(function _registerMetaHelpers(config) {

    Handlebars.registerHelper('meta-with', function(context, options) {

        if (!options) {
            return '';
        }

        var optName = options.hash['option'];
        var opt = new MetaOptionConfig();

        if (config && (optName in config)) {
            opt = config[optName];
        }

        context.metaOption = opt;

        return options.fn(context);
    });

    Handlebars.registerHelper('meta-ro', function(context, options) {

        if (!options) {
            return '';
        }

        var opt = context.metaOption;
        if (opt) {
            if(opt.readonly) {
                return options.fn(context);
            } else {
                return options.inverse(context);
            }
        } else {
            return options.inverse(context);
        }
    });

    Handlebars.registerHelper('meta-show', function(context, options) {

        if (!options) {
            return '';
        }

        var opt = context.metaOption;
        if (opt) {
            if(opt.visible) {
                return options.fn(context);
            } else {
                return options.inverse(context);
            }
        } else {
            return options.fn(context);
        }
    });
})(metaOptionCurrentConfig);
