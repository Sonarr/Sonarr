var vent = require('vent');
var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template : 'Settings/Indexers/Restriction/RestrictionDeleteViewTemplate',

    events : {
        'click .x-confirm-delete' : '_delete'
    },

    _delete : function() {
        this.model.destroy({
            wait    : true,
            success : function() {
                vent.trigger(vent.Commands.CloseModalCommand);
            }
        });
    }
});