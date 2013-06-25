"use strict";

define(
    [
        'backbone'
    ], function (Backbone) {
        return Backbone.Model.extend({

            mutators: {
                thirtyMinuteSize: function () {
                    return this.get('maxSize') * 30;
                },
                sixtyMinuteSize : function () {
                    return this.get('maxSize') * 60;
                }
            }
        });
    });
