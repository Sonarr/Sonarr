'use strict';

define(
    ['Config'],
    function (Config) {

        return function () {

            var originalInit = this.prototype.initialize;

            this.prototype.initialize = function (options) {

                options = options || {};

                if (options.tableName) {
                    this.tableName = options.tableName;
                }

                if (!this.tableName && !options.tableName) {
                    throw 'tableName is required';
                }

                _setInitialState.call(this);

                this.on('backgrid:sort', _storeState, this);

                if (originalInit) {
                    originalInit.call(this, options);
                }
            };

            var _setInitialState = function () {
                var key = Config.getValue('{0}.sortKey'.format(this.tableName), this.state.sortKey);
                var direction = Config.getValue('{0}.sortDirection'.format(this.tableName), this.state.order);
                var order = parseInt(direction, 10);

                this.state.sortKey = key;
                this.state.order = order;
            };

            var _storeState = function (column, sortDirection) {
                var order = _convertDirectionToInt(sortDirection);
                var sortKey = column.has('sortValue') ? column.get('sortValue') : column.get('name');

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
