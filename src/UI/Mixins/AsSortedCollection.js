var _ = require('underscore');
var Config = require('../Config');

module.exports = function() {

    var originalSetSorting = this.prototype.setSorting;

    this.prototype.setSorting = function(sortKey, order, options) {
        var sortMapping = this._getSortMapping(sortKey);

        options = _.defaults({ sortValue : sortMapping.sortValue }, options || {});

        return originalSetSorting.call(this, sortMapping.sortKey, order, options);
    };

    this.prototype._getSortMappings = function() {
        var result = {};

        if (this.sortMappings) {
            _.each(this.sortMappings, function(values, key) {
                var item = {
                    name      : key,
                    sortKey   : values.sortKey || key,
                    sortValue : values.sortValue
                };
                result[key] = item;
                result[item.sortKey] = item;
            });
        }

        return result;
    };

    this.prototype._getSortMapping = function(key) {
        var sortMappings = this._getSortMappings();

        return sortMappings[key] || {
                name    : key,
                sortKey : key
            };
    };

    this.prototype._getSecondarySorting = function() {
        var sortKey = this.state.secondarySortKey;
        var sortOrder = this.state.secondarySortOrder || -1;

        if (!sortKey || sortKey === this.state.sortKey) {
            return null;
        }

        var sortMapping = this._getSortMapping(sortKey);

        if (!sortMapping.sortValue) {
            sortMapping.sortValue = function(model, attr) {
                return model.get(attr);
            };
        }

        return {
            key       : sortKey,
            order     : sortOrder,
            sortValue : sortMapping.sortValue
        };
    };

    this.prototype._makeComparator = function(sortKey, order, sortValue) {
        var state = this.state;
        var secondarySorting = this._getSecondarySorting();

        sortKey = sortKey || state.sortKey;
        order = order || state.order;

        if (!sortKey || !order) {
            return;
        }

        if (!sortValue) {
            sortValue = function(model, attr) {
                return model.get(attr);
            };
        }

        return function(left, right) {
            var l = sortValue(left, sortKey, order);
            var r = sortValue(right, sortKey, order);
            var t;

            if (order === 1) {
                t = l;
                l = r;
                r = t;
            }

            if (l === r) {

                if (secondarySorting) {
                    var ls = secondarySorting.sortValue(left, secondarySorting.key, order);
                    var rs = secondarySorting.sortValue(right, secondarySorting.key, order);
                    var ts;

                    if (secondarySorting.order === 1) {
                        ts = ls;
                        ls = rs;
                        rs = ts;
                    }

                    if (ls === rs) {
                        return 0;
                    }

                    if (ls < rs) {
                        return -1;
                    }

                    return 1;
                }

                return 0;
            }

            else if (l < r) {
                return -1;
            }

            return 1;
        };
    };

    return this;
};
