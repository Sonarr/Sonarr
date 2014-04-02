﻿'use strict';
define(
    [
        'backbone',
        'System/StatusModel'
    ], function (Backbone, StatusModel) {
        return Backbone.Model.extend({
            url: function () {
                return StatusModel.get('urlBase') + '/api/log/file/' + this.get('filename');
            },

            parse: function (contents) {
                var response = {};
                response.contents = contents;
                return response;
            }
        });
    });
