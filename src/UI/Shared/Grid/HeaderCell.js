'use strict';

define(
    [
        'backgrid'
    ], function (Backgrid) {

        Backgrid.NzbDroneHeaderCell = Backgrid.HeaderCell.extend({

            events: {
                'click': 'onClick'
            },

            _originalInit: Backgrid.HeaderCell.prototype.initialize,

            initialize: function (options) {
                this._originalInit.call(this, options);

                this.listenTo(this.collection, 'drone:sort', this.render);
            },

            render: function () {
                this.$el.empty();
                this.$el.append(this.column.get('label'));

                var column = this.column;
                var sortable = Backgrid.callByNeed(column.sortable(), column, this.collection);

                if (sortable)
                {
                    this.$el.addClass('sortable');
                    this.$el.append(' <i class="pull-right"></i>');
                }

                //Do we need this?
                this.$el.addClass(column.get('name'));

                this.delegateEvents();
                this.direction(column.get('direction'));

                if (this.collection.state) {
                    var key = this.collection.state.sortKey;
                    var order = this.collection.state.order;

                    if (key === this.column.get('name')) {
                        this._setSortIcon(order);
                    }

                    else {
                        this._removeSortIcon();
                    }
                }

                return this;
            },

            direction: function (dir) {
                this.$el.children('i').removeClass('icon-sort-up icon-sort-down');

                if (arguments.length) {
                    if (dir)
                    {
                        this._setSortIcon(dir);
                    }

                    this.column.set('direction', dir);
                }

                var columnDirection = this.column.get('direction');

                if (!columnDirection && this.collection.state) {
                    var key = this.collection.state.sortKey;
                    var order = this.collection.state.order;

                    if (key === this.column.get('name')) {
                        columnDirection = order;
                    }
                }

                return columnDirection;
            },

            onClick: function (e) {
                e.preventDefault();

                var collection = this.collection;
                var event = 'backgrid:sort';

                function toggleSort(header, col) {
                    collection.state.sortKey = col.get('name');
                    var direction = header.direction();
                    if (direction === 'ascending' || direction === -1)
                    {
                        collection.state.order = 'descending';
                        collection.trigger(event, col, 'descending');
                    }
                    else
                    {
                        collection.state.order = 'ascending';
                        collection.trigger(event, col, 'ascending');
                    }
                }

                var column = this.column;
                var sortable = Backgrid.callByNeed(column.sortable(), column, this.collection);
                if (sortable) {
                    toggleSort(this, column);
                }
            },

            _resetCellDirection: function (columnToSort, direction) {
                if (columnToSort !== this.column)
                {
                    this.direction(null);
                }
                else
                {
                    this.direction(direction);
                }
            },

            _convertDirectionToIcon: function (dir) {
                if (dir === 'ascending' || dir === -1) {
                    return 'icon-sort-up';
                }

                return 'icon-sort-down';
            },

            _setSortIcon: function (dir) {
                this._removeSortIcon();
                this.$el.children('i').addClass(this._convertDirectionToIcon(dir));
            },

            _removeSortIcon: function () {
                this.$el.children('i').removeClass('icon-sort-up icon-sort-down');
            }
        });

        return Backgrid.NzbDroneHeaderCell;
    });
