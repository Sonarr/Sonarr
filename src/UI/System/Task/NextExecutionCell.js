var NzbDroneCell = require('../../Cells/NzbDroneCell');
var moment = require('moment');
var UiSettings = require('../../Shared/UiSettingsModel');

module.exports = NzbDroneCell.extend({
    className : 'next-execution-cell',

    render : function() {
        this.$el.empty();

        var interval = this.model.get('interval');
        var nextExecution = moment(this.model.get('nextExecution'));

        if (interval === 0) {
            this.$el.html('-');
        } else if (moment().isAfter(nextExecution)) {
            this.$el.html('now');
        } else {
            var result = '<span title="{0}">{1}</span>';
            var tooltip = nextExecution.format(UiSettings.longDateTime());
            var text;

            if (UiSettings.get('showRelativeDates')) {
                text = nextExecution.fromNow();
            } else {
                text = nextExecution.format(UiSettings.shortDateTime());
            }

            this.$el.html(result.format(tooltip, text));
        }

        return this;
    }
});