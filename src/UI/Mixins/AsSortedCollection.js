'use strict';

define(
    ['underscore', 'Config'],
    function (_, Config) {

        return function () {

            this.prototype._getSortMappings = function () {
                var result = {};
                
                if (this.sortMappings) {
                    _.each(this.sortMappings, function (values, key) {
                        var item = {
                            name: key,
                            sortKey: values.sortKey || key,
                            sortValue: values.sortValue
                        };
                        result[key] = item;
                        result[item.sortKey] = item;
                    });
                }

                return result;
            };

            this.prototype._getSortMapping = function (key) {
                var sortMappings = this._getSortMappings();

                return sortMappings[key] || { name: key, sortKey: key };
            };

            var originalSetSorting = this.prototype.setSorting;
            this.prototype.setSorting = function (sortKey, order, options) {
                var sortMapping = this._getSortMapping(sortKey);

                options = _.defaults({ sortValue: sortMapping.sortValue }, options || {});

                return originalSetSorting.call(this, sortMapping.sortKey, order, options);
            };

            return this;
        };
    }
);
