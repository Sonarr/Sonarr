'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {
        return Backbone.Model.extend({

            defaults: {
                seasonNumber: 0
            },

            initialize: function () {
                this.set('id', this.get('seasonNumber'));
            }
        });
    });

