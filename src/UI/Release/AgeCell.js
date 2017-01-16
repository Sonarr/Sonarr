var moment = require('moment');
var Backgrid = require('backgrid');
var UiSettings = require('../Shared/UiSettingsModel');
var FormatHelpers = require('../Shared/FormatHelpers');

module.exports = Backgrid.Cell.extend({
    className : 'age-cell',

    render : function() {
        var age = this.model.get('age');
        var ageHours = this.model.get('ageHours');
        var ageMinutes = this.model.get('ageMinutes');
        var published = moment(this.model.get('publishDate'));
        var publishedFormatted = published.format('{0} {1}'.format(UiSettings.get('shortDateFormat'), UiSettings.time(true, true)));
        var formatted = age;
        var suffix = FormatHelpers.plural(age, 'day');

        if (age < 2) {
            formatted = ageHours.toFixed(1);
            suffix = FormatHelpers.plural(Math.round(ageHours), 'hour');
        }

        if (ageHours < 2) {
            formatted = ageMinutes.toFixed(1);
            suffix = FormatHelpers.plural(Math.round(ageMinutes), 'minute');
        }

        this.$el.html('<div title="{2}">{0} {1}</div>'.format(formatted, suffix, publishedFormatted));

        this.delegateEvents();
        return this;
    }
});