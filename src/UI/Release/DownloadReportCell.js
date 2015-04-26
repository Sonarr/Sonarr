var Backgrid = require('backgrid');

module.exports = Backgrid.Cell.extend({
    className : 'download-report-cell',

    events : {
        'click' : '_onClick'
    },

    _onClick : function() {
        if (!this.model.get('downloadAllowed')) {
            return;
        }

        var self = this;

        this.$el.html('<i class="icon-sonarr-spinner fa-spin" />');

        //Using success callback instead of promise so it
        //gets called before the sync event is triggered
        this.model.save(null, {
            success : function() {
                self.model.set('queued', true);
            }
        });
    },

    render : function() {
        this.$el.empty();

        if (this.model.get('queued')) {
            this.$el.html('<i class="icon-sonarr-downloading" title="Added to downloaded queue" />');
        } else if (this.model.get('downloadAllowed')) {
            this.$el.html('<i class="icon-sonarr-download" title="Add to download queue" />');
        } else {
            this.className = 'no-download-report-cell';
        }

        return this;
    }
});