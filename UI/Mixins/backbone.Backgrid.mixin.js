"use strict";

Backgrid.Column.prototype.defaults = {
    name      : undefined,
    label     : undefined,
    sortable  : true,
    editable  : false,
    renderable: true,
    formatter : undefined,
    cell      : undefined,
    headerCell: 'nzbDrone'
};

Backgrid.TemplateBackedCell = Backgrid.Cell.extend({
    className: '',
    template : 'Series/Index/Table/ControlsColumnTemplate',

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
            this.$el.addClass('clickable');
            this.$el.append(" <i class='pull-right'></i>");

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
            if (this._direction) {
                this.$el.children('i').removeClass(this._convertDirectionToIcon(this._direction));
            }
            if (dir) {
                this.$el.children('i').addClass(this._convertDirectionToIcon(dir));
            }
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
                    else if (leftVal > rightVal) {
                        return -1;
                    }
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
                    else if (leftVal < rightVal) {
                        return -1;
                    }
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

    template: 'Shared/BackgridPaginatorTemplate',

    events: {
        "click .pager-btn": "changePage"
    },

    windowSize: 1,

    fastForwardHandleLabels: {
        first: 'icon-fast-backward',
        prev : 'icon-backward',
        next : 'icon-forward',
        last : 'icon-fast-forward'
    },

    changePage: function (e) {
        e.preventDefault();

        var target = $(e.target);

        if (target.closest('li').hasClass('disabled')) {
            return;
        }

        var label = target.attr('data-action');
        var ffLabels = this.fastForwardHandleLabels;

        var collection = this.collection;

        if (ffLabels) {
            switch (label) {
                case 'first':
                    collection.getFirstPage();
                    return;
                case 'prev':
                    if (collection.hasPrevious()) {
                        collection.getPreviousPage();
                    }
                    return;
                case 'next':
                    if (collection.hasNext()) {
                        collection.getNextPage();
                    }
                    return;
                case 'last':
                    collection.getLastPage();
                    return;
            }
        }

        var state = collection.state;
        var pageIndex = $(e.target).text() * 1;
        collection.getPage(state.firstPage === 0 ? pageIndex - 1 :pageIndex);
    },

    makeHandles: function () {

        var handles = [];
        var collection = this.collection;
        var state = collection.state;

        // convert all indices to 0-based here
        var firstPage = state.firstPage;
        var lastPage = +state.lastPage;
        lastPage = Math.max(0, firstPage ? lastPage - 1 : lastPage);
        var currentPage = Math.max(state.currentPage, state.firstPage);
        currentPage = firstPage ? currentPage - 1 : currentPage;
        var windowStart = Math.floor(currentPage / this.windowSize) * this.windowSize;
        var windowEnd = Math.min(lastPage + 1, windowStart + this.windowSize);

        if (collection.mode !== "infinite") {
            for (var i = windowStart; i < windowEnd; i++) {
                handles.push({
                    label: i + 1,
                    title: "No. " + (i + 1),
                    className: currentPage === i ? "active" : undefined,
                    pageNumber: i + 1
                });
            }
        }

        var ffLabels = this.fastForwardHandleLabels;
        if (ffLabels) {

            if (ffLabels.prev) {
                handles.unshift({
                    label: ffLabels.prev,
                    className: collection.hasPrevious() ? void 0 : "disabled",
                    action: 'prev'
                });
            }

            if (ffLabels.first) {
                handles.unshift({
                    label: ffLabels.first,
                    className: collection.hasPrevious() ? void 0 : "disabled",
                    action: 'first'
                });
            }

            if (ffLabels.next) {
                handles.push({
                    label: ffLabels.next,
                    className: collection.hasNext() ? void 0 : "disabled",
                    action: 'next'
                });
            }

            if (ffLabels.last) {
                handles.push({
                    label: ffLabels.last,
                    className: collection.hasNext() ? void 0 : "disabled",
                    action: 'last'
                });
            }
        }

        return handles;
    },

    render: function () {
        this.$el.empty();

        var templateFunction = Marionette.TemplateCache.get(this.template);

        this.$el.html(templateFunction({
            handles: this.makeHandles()
        }));

        this.delegateEvents();

        return this;
    }
});
