var vent = require('vent');
var Backgrid = require('backgrid');

module.exports = Backgrid.Row.extend({
    className : 'log-row',

    events : {
        'click' : '_showDetails'
    },

    _showDetails : function() {
        vent.trigger(vent.Commands.ShowLogDetails, { model : this.model });
    }
});