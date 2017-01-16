var NzbDroneCell = require('./NzbDroneCell');
var moment = require('moment');
var FormatHelpers = require('../Shared/FormatHelpers');
var UiSettings = require('../Shared/UiSettingsModel');

module.exports = NzbDroneCell.extend({
    className : 'relative-time-cell',

    render : function() {

        var dateStr = this.model.get(this.column.get('name'));

        if (dateStr) {
            var date = moment(dateStr);
            var result = '<span title="{0}">{1}</span>';
            var tooltip = date.format(UiSettings.longDateTime());
            var text;

            if (UiSettings.get('showRelativeDates')) {
                text = date.fromNow();
            } else {
                text = date.format(UiSettings.shortDateTime());
            }

            this.$el.html(result.format(tooltip, text));
        }

        return this;
    }
});