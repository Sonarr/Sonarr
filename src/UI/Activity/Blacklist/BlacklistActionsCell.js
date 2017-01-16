var vent = require('vent');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var BlacklistDetailsLayout = require('./Details/BlacklistDetailsLayout');

module.exports = NzbDroneCell.extend({
    className : 'blacklist-actions-cell',

    events : {
        'click .x-details' : '_details',
        'click .x-delete'  : '_delete'
    },

    render : function() {
        this.$el.empty();
        this.$el.html('<i class="icon-sonarr-info x-details"></i>' +
                      '<i class="icon-sonarr-delete x-delete"></i>');

        return this;
    },

    _details : function() {
        vent.trigger(vent.Commands.OpenModalCommand, new BlacklistDetailsLayout({ model : this.model }));
    },

    _delete : function() {
        this.model.destroy();
    }
});
