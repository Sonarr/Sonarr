var NzbDroneCell = require('../../Cells/NzbDroneCell');
var moment = require('moment');
var UiSettingsModel = require('../../Shared/UiSettingsModel');
var FormatHelpers = require('../../Shared/FormatHelpers');

module.exports = NzbDroneCell.extend({
    className : 'timeleft-cell',

    render : function() {
        this.$el.empty();

        if (this.cellValue) {
            var status = this.cellValue.get('status').toLowerCase();
            var ect = this.cellValue.get('estimatedCompletionTime');
            var time = '{0} at {1}'.format(FormatHelpers.relativeDate(ect), moment(ect).format(UiSettingsModel.time(true, false)));

            if (status === 'pending') {
                this.$el.html('<div title="Delaying download till {0}">-</div>'.format(time));
            } else if (status === 'downloadclientunavailable') {
                this.$el.html('<div title="Retrying download at {0}">-</div>'.format(time));
            } else {
                var timeleft = this.cellValue.get('timeleft');
                var totalSize = FormatHelpers.bytes(this.cellValue.get('size'), 2);
                var remainingSize = FormatHelpers.bytes(this.cellValue.get('sizeleft'), 2);

                if (timeleft === undefined) {
                    this.$el.html('-');
                } else {
                    var duration = moment.duration(timeleft);
                    var days = duration.get('days');
                    var hours = FormatHelpers.pad(duration.get('hours'), 2);
                    var minutes = FormatHelpers.pad(duration.get('minutes'), 2);
                    var seconds = FormatHelpers.pad(duration.get('seconds'), 2);

                    var formattedTime = '{0}:{1}:{2}'.format(hours, minutes, seconds);

                    if (days > 0) {
                        formattedTime = days + '.' + formattedTime;
                    }

                    this.$el.html('<span title="{1} / {2}">{0}</span>'.format(formattedTime, remainingSize, totalSize));
                }
            }
        }

        return this;
    }
});
