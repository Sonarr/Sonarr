﻿'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {
        return Backbone.Model.extend({
            url: function () {
                return '/log/' + this.get('filename');
            },

            parse: function (contents) {
                var response = {};
                response.contents = contents;
                return response;
            }
        });
    });
