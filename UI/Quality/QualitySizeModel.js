"use strict";

define(
    [
        'backbone'
    ], function (Backbone) {
        return Backbone.Model.extend({

            mutators: {
                thirtyMinuteSize: function () {
                    var maxSize = this.get('maxSize');

                    if (maxSize === 0) {
                        return 'No Limit';
                    }

                    return (maxSize * 1024 * 1024 * 30).bytes(1);
                },
                sixtyMinuteSize : function () {
                    var maxSize = this.get('maxSize');

                    if (maxSize === 0) {
                        return 'No Limit';
                    }

                    return (maxSize * 1024 * 1024 * 60).bytes(1);
                }
            }
        });
    });
