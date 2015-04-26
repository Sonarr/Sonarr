var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
    className : 'protocol-cell',

    render : function() {
        var protocol = this.model.get('protocol') || 'Unknown';
        var label = '??';

        if (protocol) {
            if (protocol === 'torrent') {
                label = 'torrent';
            } else if (protocol === 'usenet') {
                label = 'nzb';
            }

            this.$el.html('<div class="label label-default protocol-{0}" title="{0}">{1}</div>'.format(protocol, label));
        }

        this.delegateEvents();

        return this;
    }
});