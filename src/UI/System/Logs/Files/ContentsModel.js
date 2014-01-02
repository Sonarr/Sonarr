﻿'use strict';
define(
    [
        'backbone',
        'System/StatusModel'
    ], function (Backbone, StatusModel) {
        return Backbone.Model.extend({
            url: function () {
                return StatusModel.get('urlBase') + '/log/' + this.get('filename');
            },

            parse: function (contents) {
                var response = {};
                response.contents = contents;
                return response;
            }
        });
    });
