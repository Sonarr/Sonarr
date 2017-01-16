var vent = require('../../vent');
var NzbDroneCell = require('../../Cells/NzbDroneCell');
var SelectSeriesLayout = require('../Series/SelectSeriesLayout');

module.exports = NzbDroneCell.extend({
    className : 'series-title-cell editable',

    events : {
        'click' : '_onClick'
    },

    render : function() {
        this.$el.empty();

        var series = this.model.get('series');

        if (series)
        {
            this.$el.html(series.title);
        }

        this.delegateEvents();
        return this;
    },

    _onClick : function () {
        var view = new SelectSeriesLayout();

        this.listenTo(view, 'manualimport:selected:series', this._setSeries);

        vent.trigger(vent.Commands.OpenModal2Command, view);
    },

    _setSeries : function (e) {
        if (this.model.has('series') && e.model.id === this.model.get('series').id) {
            return;
        }

        this.model.set({
            series       : e.model.toJSON(),
            seasonNumber : undefined,
            episodes     : []
        });
    }
});