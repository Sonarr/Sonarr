'use strict';
define(
    [
        'backbone',
        'Shared/Toolbar/ButtonModel'
    ], function (Backbone, ButtonModel) {
        return Backbone.Collection.extend({
            model: ButtonModel
        });
    });

