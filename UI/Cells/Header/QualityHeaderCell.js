'use strict';

define(
    [
        'backgrid',
        'Shared/Grid/HeaderCell'
    ], function (Backgrid, NzbDroneHeaderCell) {

        Backgrid.QualityHeaderCell = NzbDroneHeaderCell.extend({
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
                var leftWeight = leftVal.quality.weight;
                var rightWeight = rightVal.quality.weight;

                if (!leftWeight && !rightWeight) {
                    return 0;
                }

                if (!leftWeight) {
                    return -1;
                }

                if (!rightWeight) {
                    return 1;
                }

                if (leftWeight === rightWeight) {
                    return 0;
                }

                if (leftWeight > rightWeight) {
                    return -1;
                }

                return 1;
            }
        });

        return Backgrid.QualityHeaderCell;
    });
