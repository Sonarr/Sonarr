var NzbDroneCell = require('../../Cells/NzbDroneCell');
var moment = require('moment');

module.exports = NzbDroneCell.extend({
    className : 'task-interval-cell',

    render : function() {
        this.$el.empty();

        var interval = this.model.get('interval');
        var duration = moment.duration(interval, 'minutes').humanize().replace(/an?(?=\s)/, '1');

        if (interval === 0) {
            this.$el.html('disabled');
        } else {
            this.$el.html(duration);
        }

        return this;
    }
});