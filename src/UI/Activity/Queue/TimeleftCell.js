var NzbDroneCell = require('../../Cells/NzbDroneCell');
var moment = require('moment');
var UiSettingsModel = require('../../Shared/UiSettingsModel');
var FormatHelpers = require('../../Shared/FormatHelpers');

module.exports = NzbDroneCell.extend({
    className : 'timeleft-cell',

    render : function() {
        this.$el.empty();

        if (this.cellValue) {
            if (this.cellValue.get('status').toLowerCase() === 'pending') {
                var ect = this.cellValue.get('estimatedCompletionTime');
                var time = '{0} at {1}'.format(FormatHelpers.relativeDate(ect), moment(ect).format(UiSettingsModel.time(true, false)));
                this.$el.html('<div title="Delaying download till {0}">-</div>'.format(time));
                return this;
            }

            var timeleft = this.cellValue.get('timeleft');
            var totalSize = FormatHelpers.bytes(this.cellValue.get('size'), 2);
            var remainingSize = FormatHelpers.bytes(this.cellValue.get('sizeleft'), 2);

            if (timeleft === undefined) {
                this.$el.html('-');
            } else {
                this.$el.html('<span title="{1} / {2}">{0}</span>'.format(timeleft, remainingSize, totalSize));
            }
        }

        return this;
    }
});