var NzbDroneCell = require('../../Cells/NzbDroneCell');

module.exports = NzbDroneCell.extend({
    className : 'progress-cell',

    render : function() {
        this.$el.empty();

        if (this.cellValue) {

            var status = this.model.get('status').toLowerCase();

            if (status === 'downloading') {
                var progress = 100 - (this.model.get('sizeleft') / this.model.get('size') * 100);

                this.$el.html('<div class="progress" title="{0}%">'.format(progress.toFixed(1)) +
                              '<div class="progress-bar progress-bar-purple" style="width: {0}%;"></div></div>'.format(progress));
            }
        }

        return this;
    }
});
