'use strict';

define(
    ['Config'],
    function (Config) {

        return function () {

            var originalInit = this.prototype.initialize;

            this.prototype.initialize = function () {

                if (!this.tableName) {
                    throw 'tableName is required';
                }

                _setState.call(this);

                this.on('backgrid:sort', _storeState, this);

                if (originalInit) {
                    originalInit.call(this);
                }
            };

            var _setState = function () {
                var key = Config.getValue('{0}.sortKey'.format(this.tableName), this.state.sortKey);
                var direction = Config.getValue('{0}.sortDirection'.format(this.tableName), this.state.order);
                var order = parseInt(direction, 10);

                this.state.sortKey = key;
                this.state.order = order;
            };

            var _storeState = function (sortKey, sortDirection) {
                var order = _convertDirectionToInt(sortDirection);

                Config.setValue('{0}.sortKey'.format(this.tableName), sortKey);
                Config.setValue('{0}.sortDirection'.format(this.tableName), order);
            };

            var _convertDirectionToInt = function (dir) {
                if (dir === 'ascending') {
                    return '-1';
                }

                return '1';
            };

            return this;
        };
    }
);
