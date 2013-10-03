'use strict';

define(
    [
        'backgrid',
        'Shared/Grid/HeaderCell'
    ], function (Backgrid, NzbDroneHeaderCell) {

        Backgrid.DateHeaderCell = NzbDroneHeaderCell.extend({
            events: {
                'click': 'onClick'
            },

            onClick: function (e) {
                e.preventDefault();

                var self = this;
                var columnName = this.column.get('name');

                if (this.column.get('sortable')) {
                    if (this.direction() === 'ascending') {
                        this.sort(columnName, 'descending', function (left, right) {
                            var leftVal = left.get(columnName);
                            var rightVal = right.get(columnName);

                            return self._comparator(leftVal, rightVal);
                        });
                    }
                    else {
                        this.sort(columnName, 'ascending', function (left, right) {
                            var leftVal = left.get(columnName);
                            var rightVal = right.get(columnName);

                            return self._comparator(rightVal, leftVal);
                        });
                    }
                }
            },

            _comparator: function (leftVal, rightVal) {
                if (!leftVal && !rightVal) {
                    return 0;
                }

                if (!leftVal) {
                    return -1;
                }

                if (!rightVal) {
                    return 1;
                }

                if (leftVal === rightVal) {
                    return 0;
                }

                if (leftVal > rightVal) {
                    return -1;
                }

                return 1;
            }
        });

        return Backgrid.DateHeaderCell;
    });
