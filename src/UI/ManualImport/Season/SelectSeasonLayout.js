var vent = require('vent');
var Marionette = require('marionette');

module.exports = Marionette.Layout.extend({
    template  : 'ManualImport/Season/SelectSeasonLayoutTemplate',

    events : {
        'change .x-select-season' : '_selectSeason'
    },

    initialize : function(options) {

        this.templateHelpers = {
            seasons : options.seasons
        };
    },

    _selectSeason : function (e) {
        var seasonNumber = parseInt(e.target.value, 10);

        if (seasonNumber === -1) {
            return;
        }

        this.trigger('manualimport:selected:season', { seasonNumber: seasonNumber });
        vent.trigger(vent.Commands.CloseModal2Command);
    }
});