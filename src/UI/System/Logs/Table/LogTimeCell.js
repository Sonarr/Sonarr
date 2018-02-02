var NzbDroneCell = require('../../../Cells/NzbDroneCell');
var moment = require('moment');
var FormatHelpers = require('../../../Shared/FormatHelpers');
var UiSettings = require('../../../Shared/UiSettingsModel');

module.exports = NzbDroneCell.extend({
    className : 'log-time-cell',

    render : function() {
        var dateStr = this._getValue();
        var date = moment(dateStr);
        var diff = date.diff(moment().utcOffset(date.utcOffset()).startOf('day'), 'days', true);
        var result = '<span title="{0}">{1}</span>';
        var tooltip = date.format(UiSettings.longDateTime(true));
        var text;

        if (diff > 0 && diff < 1) {
            text = date.format(UiSettings.time(true, false));
        } else {
            if (UiSettings.get('showRelativeDates')) {
                text = FormatHelpers.relativeDate(dateStr);
            } else {
                text = date.format(UiSettings.get('shortDateFormat'));
            }
        }

        this.$el.html(result.format(tooltip, text));

        return this;
    }
});
