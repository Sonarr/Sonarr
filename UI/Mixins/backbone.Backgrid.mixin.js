Backgrid.SeriesStatusCell = Backgrid.Cell.extend({
    className: "series-status-cell",

    render: function () {
        this.$el.empty();
        var monitored = this.model.get('monitored');
        var status = this.model.get('status');

        if (!monitored) {
            this.$el.html('<i class="icon-pause grid-icon" title="Not Monitored"></i>');
        }
        else if (status === 0) {
            this.$el.html('<i class="icon-play grid-icon" title="Continuing"></i>');
        }

        else {
            this.$el.html('<i class="icon-stop grid-icon" title="Ended"></i>');
        }

        return this;
    }
});

Backgrid.AirDateCell = Backgrid.Cell.extend({
    className: "air-date-cell",

    render: function () {
        this.$el.empty();
        var airDate = this.model.get(this.column.get("name"));

        this.$el.html(bestDateString(airDate));

        return this;
    }
});

Backgrid.EpisodeProgressCell = Backgrid.Cell.extend({
    className: "episode-progress-cell",
    template: 'Series/EpisodeProgressTemplate',

    render: function () {
        var data = this.model.toJSON();
        var html = Marionette.Renderer.render(this.template, data);
        this.$el.html(html);

        return this;
    }
});

Backgrid.ControlsColumnCell = Backgrid.Cell.extend({
    className: "controls-cell",
    template: 'Series/Index/Table/ControlsColumnTemplate',

    render: function () {
        var data = this.model.toJSON();
        var html = Marionette.Renderer.render(this.template, data);
        this.$el.html(html);

        return this;
    }
});

Backgrid.SeriesIndexTableRow = Backgrid.Row.extend({
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

Backgrid.NzbDroneHeaderCell = Backgrid.HeaderCell.extend({
    events: {
        'click': 'onClick'
    },

    render: function () {
        this.$el.empty();
        var test = this.column.get('label');
        this.$el.append(this.column.get("label"));

        if (this.column.get('sortable')) {
            this.$el.append(" <i class='icon-sort pull-right'></i>");
        }
        this.delegateEvents();
        return this;
    },

    direction: function (dir) {
        if (arguments.length) {
            if (this._direction) this.$el.children('i').removeClass(this._convertDirectionToIcon(this._direction));
            if (dir) this.$el.children('i').addClass(this._convertDirectionToIcon(dir));
            this._direction = dir;
        }

        return this._direction;
    },

    _convertDirectionToIcon: function (dir) {
        if (dir === 'ascending') {
            return 'icon-sort-up';
        }

        return 'icon-sort-down';
    }
});