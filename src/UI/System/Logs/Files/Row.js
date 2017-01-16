var vent = require('vent');
var Backgrid = require('backgrid');

module.exports = Backgrid.Row.extend({
    className : 'log-file-row',

    events : {
        'click' : '_showDetails'
    },

    _showDetails : function() {
        vent.trigger(vent.Commands.ShowLogFile, { model : this.model });
    }
});