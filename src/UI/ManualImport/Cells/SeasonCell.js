var vent = require('../../vent');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var SelectSeasonLayout = require('../Season/SelectSeasonLayout');

module.exports = NzbDroneCell.extend({
    className : 'season-cell',

    events : {
        'click' : '_onClick'
    },

    render : function() {
        this.$el.empty();

        if (this.model.has('seasonNumber')) {
            this.$el.html(this.model.get('seasonNumber'));
        }

        this.delegateEvents();
        return this;
    },

    _onClick : function () {
        var series = this.model.get('series');

        if (!series) {
            return;
        }

        var view = new SelectSeasonLayout({ seasons: series.seasons });

        this.listenTo(view, 'manualimport:selected:season', this._setSeason);

        vent.trigger(vent.Commands.OpenModal2Command, view);
    },

    _setSeason : function (e) {
        if (this.model.has('seasonNumber') && e.seasonNumber === this.model.get('seasonNumber')) {
            return;
        }

        this.model.set({
            seasonNumber : e.seasonNumber,
            episodes     : []
        });
    }
});