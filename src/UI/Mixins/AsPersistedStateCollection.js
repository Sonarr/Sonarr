'use strict';

define(
    ['underscore', 'Config'],
    function (_, Config) {

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

                this.on('backgrid:sort', _storeStateFromBackgrid, this);
                this.on('drone:sort', _storeState, this);

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

            var _storeStateFromBackgrid = function (column, sortDirection) {
                var order = _convertDirectionToInt(sortDirection);
                var sortKey = column.has('sortValue') && _.isString(column.get('sortValue')) ? column.get('sortValue') : column.get('name');

                Config.setValue('{0}.sortKey'.format(this.tableName), sortKey);
                Config.setValue('{0}.sortDirection'.format(this.tableName), order);
            };

            var _storeState = function (sortModel, sortDirection) {
                var order = _convertDirectionToInt(sortDirection);
                var sortKey = sortModel.get('name');

                Config.setValue('{0}.sortKey'.format(this.tableName), sortKey);
                Config.setValue('{0}.sortDirection'.format(this.tableName), order);
            };

            var _convertDirectionToInt = function (dir) {
                if (dir === 'ascending') {
                    return '-1';
                }

                return '1';
            };

            _.extend(this.prototype, {
                initialSort: function () {
                    var key = this.state.sortKey;
                    var order = this.state.order;

                    if (this.sorters && this.sorters[key] && this.mode === 'client') {
                        var sortValue = this[key];

                        var comparator = this._makeComparator(key, order, sortValue);
                        this.fullCollection.comparator = comparator;
                        this.fullCollection.sort();
                    }
                }
            });

            return this;
        };
    }
);
