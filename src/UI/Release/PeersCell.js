var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
    className : 'peers-cell',

    render : function() {
        if (this.model.get('protocol') === 'torrent') {
            var seeders = this.model.get('seeders');
            var leechers = this.model.get('leechers');

            var level = 'danger';

            if (seeders > 0) {
                level = 'warning';
            } else if (seeders > 10) {
                level = 'info';
            } else if (seeders > 50) {
                level = 'primary';
            }

            var txtSeeders = seeders;
            var txtLeechers = leechers;
            if (typeof seeders === 'undefined') {
                txtSeeders = 'unknown';
                seeders = '-';
            }

            if (typeof leechers === 'undefined') {
                txtLeechers = 'unknown';
                leechers = '-';
            }

            this.$el.html('<div class="label label-{0}" title="{1} seeders, {2} leechers">{3} / {4}</div>'.format(level, txtSeeders, txtLeechers, seeders, leechers));
        }

        this.delegateEvents();

        return this;
    }
});