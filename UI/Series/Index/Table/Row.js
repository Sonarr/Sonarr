NzbDrone.Series.Index.Table.Row = Backgrid.Row.extend({
    events: {
        'click .x-edit'  : 'editSeries',
        'click .x-remove': 'removeSeries'
    },

    editSeries: function () {
        var view = new NzbDrone.Series.Edit.EditSeriesView({ model: this.model});

        NzbDrone.vent.trigger(NzbDrone.Events.OpenModalDialog, {
            view: view
        });
    },

    removeSeries: function () {
        var view = new NzbDrone.Series.Delete.DeleteSeriesView({ model: this.model });
        NzbDrone.vent.trigger(NzbDrone.Events.OpenModalDialog, {
            view: view
        });
    }
});