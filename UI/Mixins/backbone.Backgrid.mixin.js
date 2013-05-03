"use strict";

Backgrid.TemplateBackedCell = Backgrid.Cell.extend({
    className: '',
    template: 'Series/Index/Table/ControlsColumnTemplate',

    render: function () {
        var data = this.model.toJSON();
        var templateFunction = Marionette.TemplateCache.get(this.template);
        var html = templateFunction(data);
        this.$el.html(html);

        return this;
    }
});

Backgrid.NzbDroneHeaderCell = Backgrid.HeaderCell.extend({
    events: {
        'click': 'onClick'
    },

    render: function () {
        this.$el.empty();
        this.$el.append(this.column.get("label"));

        if (this.column.get('sortable')) {
            this.$el.append(" <i class='icon-sort pull-right'></i>");

            if (this.collection.state) {
                var sortKey = this.collection.state.sortKey;
                var sortDir = this._convertIntToDirection(this.collection.state.order);

                if (sortKey === this.column.get('name')) {
                    this.$el.children('i').addClass(this._convertDirectionToIcon(sortDir));
                    this._direction = sortDir;
                }
            }
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

    onClick: function (e) {
        e.preventDefault();

        var columnName = this.column.get("name");

        if (this.column.get("sortable")) {
            if (this.direction() === "ascending") {
                this.sort(columnName, "descending", function (left, right) {
                    var leftVal = left.get(columnName);
                    var rightVal = right.get(columnName);
                    if (leftVal === rightVal) {
                        return 0;
                    }
                    else if (leftVal > rightVal) { return -1; }
                    return 1;
                });
            }
            else {
                this.sort(columnName, "ascending", function (left, right) {
                    var leftVal = left.get(columnName);
                    var rightVal = right.get(columnName);
                    if (leftVal === rightVal) {
                        return 0;
                    }
                    else if (leftVal < rightVal) { return -1; }
                    return 1;
                });
            }
        }
    },

    _convertDirectionToIcon: function (dir) {
        if (dir === 'ascending') {
            return 'icon-sort-up';
        }

        return 'icon-sort-down';
    },

    _convertIntToDirection: function (dir) {
        if (dir === '-1') {
            return 'ascending';
        }

        return 'descending';
    }
});

Backgrid.NzbDronePaginator = Backgrid.Extension.Paginator.extend({

    events: {
        "click a": "changePage",
        "click i": "preventLinkClick"
    },

    windowSize: 1,

    fastForwardHandleLabels: {
        first: '<i class="icon-fast-backward"></i>',
        prev: '<i class="icon-backward"></i>',
        next: '<i class="icon-forward"></i>',
        last: '<i class="icon-fast-forward"></i>'
    },

    changePage: function (e) {
        e.preventDefault();

        var target = $(e.target);

        if (target.closest('li').hasClass('disabled')) {
            return;
        }

        if (!$(target).is('a')){
            target = target.parent('a');
        }

        var label = target.html();
        var ffLabels = this.fastForwardHandleLabels;

        var collection = this.collection;

        if (ffLabels) {
            switch (label) {
                case ffLabels.first:
                    collection.getFirstPage();
                    return;
                case ffLabels.prev:
                    if (collection.hasPrevious()) {
                        collection.getPreviousPage();
                    }
                    return;
                case ffLabels.next:
                    if (collection.hasNext()) {
                        collection.getNextPage();
                    }
                    return;
                case ffLabels.last:
                    collection.getLastPage();
                    return;
            }
        }

        var state = collection.state;
        var pageIndex = $(e.target).text() * 1;
        collection.getPage(state.firstPage === 0 ? pageIndex - 1 : pageIndex);
    },

    preventLinkClick: function (e) {
        e.preventDefault();
    }
});