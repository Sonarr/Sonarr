var Handlebars = require('handlebars');
var moment = require('moment');
var FormatHelpers = require('../../Shared/FormatHelpers');
var UiSettings = require('../../Shared/UiSettingsModel');

Handlebars.registerHelper('ShortDate', function(input) {
    if (!input) {
        return '';
    }

    var date = moment(input);
    var result = '<span title="' + date.format(UiSettings.longDateTime()) + '">' + date.format(UiSettings.get('shortDateFormat')) + '</span>';

    return new Handlebars.SafeString(result);
});

Handlebars.registerHelper('RelativeDate', function(input) {
    if (!input) {
        return '';
    }

    var date = moment(input);
    var result = '<span title="{0}">{1}</span>';
    var tooltip = date.format(UiSettings.longDateTime());
    var text;

    if (UiSettings.get('showRelativeDates')) {
        text = FormatHelpers.relativeDate(input);
    } else {
        text = date.format(UiSettings.get('shortDateFormat'));
    }

    result = result.format(tooltip, text);

    return new Handlebars.SafeString(result);
});

Handlebars.registerHelper('Day', function(input) {
    if (!input) {
        return '';
    }

    return moment(input).format('DD');
});

Handlebars.registerHelper('Month', function(input) {
    if (!input) {
        return '';
    }

    return moment(input).format('MMM');
});

Handlebars.registerHelper('StartTime', function(input) {
    if (!input) {
        return '';
    }

    return moment(input).format(UiSettings.time(false, false));
});

Handlebars.registerHelper('LTS', function(input) {
    if (!input) {
        return '';
    }

    return moment(input).format(UiSettings.time(true, true));
});

Handlebars.registerHelper('if_today', function(context, options) {
    var date = moment(context).startOf('day');
    var today = moment().startOf('day');

    if (date.isSame(today)) {
        return options.fn(this);
    }

    return options.inverse(this);
});

Handlebars.registerHelper('unless_today', function(context, options) {
    var date = moment(context).startOf('day');
    var today = moment().startOf('day');

    if (date.isSame(today)) {
        return options.inverse(this);
    }

    return options.fn(this);
});