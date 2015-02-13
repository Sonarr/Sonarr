var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
    className : 'peers-cell',

    render : function() {
        if (this.model.get('protocol') === 'torrent') {
            var seeders = this.model.get('seeders') || 0;
            var leechers = this.model.get('leechers') || 0;

            var level = 'danger';

            if (seeders > 0) {
                level = 'warning';
            } else if (seeders > 10) {
                level = 'info';
            } else if (seeders > 50) {
                level = 'primary';
            }

            this.$el.html('<div class="label label-{2}" title="{0} seeders, {1} leechers">{0} / {1}</div>'.format(seeders, leechers, level));
        }

        this.delegateEvents();

        return this;
    }
});