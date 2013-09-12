'use strict';
define(
    [
        'marionette'
    ], function (Marionette) {

        return  Marionette.ItemView.extend({
            template: 'Series/Details/InfoViewTemplate',

            initialize: function () {
                this.listenTo(this.model, 'change', this.render);
            }
        });
    });
