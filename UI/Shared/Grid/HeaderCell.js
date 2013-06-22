'use strict';

define(['app','backgrid'], function () {


    Backgrid.NzbDroneHeaderCell = Backgrid.HeaderCell.extend({
        events: {
            'click': 'onClick'
        },

        render: function () {
            this.$el.empty();
            this.$el.append(this.column.get('label'));

            if (this.column.get('sortable')) {
                this.$el.addClass('clickable');
                this.$el.append(' <i class='pull-right'></i>');

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

            var columnName = this.column.get('name');

            if (this.column.get('sortable')) {
                if (this.direction() === 'ascending') {
                    this.sort(columnName, 'descending', function (left, right) {
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
                    this.sort(columnName, 'ascending', function (left, right) {
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

    return Backgrid.NzbDroneHeaderCell;
});
