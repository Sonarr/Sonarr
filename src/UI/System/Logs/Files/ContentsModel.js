﻿'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {
        return Backbone.Model.extend({
            url: function () {
                return this.get('contentsUrl');
            },

            parse: function (contents) {
                var response = {};
                response.contents = contents;
                return response;
            }
        });
    });
