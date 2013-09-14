'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {
        return Backbone.Model.extend({
            url: window.NzbDrone.ApiRoot + '/command',

            parse: function (response) {
                response.name = response.name.toLocaleLowerCase();
                return response;
            },

            isActive: function () {
                return this.get('state') !== 'completed' && this.get('state') !== 'failed';
            },

            isSameCommand: function (command) {

                if (command.name.toLocaleLowerCase() != this.get('name').toLocaleLowerCase()) {
                    return false;
                }

                for (var key in command) {
                    if (key !== 'name' && command[key] !== this.get(key)) {
                        return false;
                    }
                }

                return true;
            }
        });
    });
