"use strict";

Backgrid.TemplateBackedCell = Backgrid.Cell.extend({
    className: '',
    template: 'Series/Index/Table/ControlsColumnTemplate',

    render: function () {
        var data = this.model.toJSON();
        var templateFunction = Marionette.TemplateCache.get(this.template);
        var html = new Handlebars.SafeString(templateFunction(data));
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