'use strict';
define(
    [
        'backbone',
        'sugar'
    ], function (Backbone) {
        return Backbone.Model.extend({
            mutators: {
                freeSpaceString: function () {
                    return this.get('freeSpace').bytes(2) + ' Free';
                }
            },

            defaults: {
                freeSpace: 0
            }
        });
    });
