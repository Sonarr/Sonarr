'use strict';
define(
    [
        'underscore',
        'backbone'
    ], function (_, Backbone) {
        return Backbone.Model.extend({
            defaults: {
                'target' : '/nzbdrone/route',
                'title'  : '',
                'active' : false,
                'tooltip': undefined
            },

            sortValue: function () {
                var sortValue = this.get('sortValue');
                if (_.isString(sortValue)) {
                    return this[sortValue];
                }
                else if (_.isFunction(sortValue)) {
                    return sortValue;
                }

                return function (model, colName) {
                    return model.get(colName);
                };
            }
        });
    });
