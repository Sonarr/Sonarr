var NzbDroneCell = require('../../../Cells/NzbDroneCell');
var moment = require('moment');
var UiSettings = require('../../../Shared/UiSettingsModel');

module.exports = NzbDroneCell.extend({
    className : 'log-time-cell',

    render : function() {
        var date = moment(this._getValue());
        this.$el.html('<span title="{1}">{0}</span>'.format(date.format(UiSettings.time(true, false)), date.format(UiSettings.longDateTime(true))));

        return this;
    }
});