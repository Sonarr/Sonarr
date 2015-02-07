var vent = require('vent');
var Marionette = require('marionette');

module.exports = Marionette.ItemView.extend({
    template       : 'Settings/Profile/DeleteProfileViewTemplate',
    events         : {"click .x-confirm-delete" : '_removeProfile'},
    _removeProfile : function(){
        this.model.destroy({wait : true}).done(function(){
            vent.trigger(vent.Commands.CloseModalCommand);
        });
    }
});