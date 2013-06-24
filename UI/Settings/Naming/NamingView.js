﻿'use strict';
define(
    [
        'marionette',
        'Settings/Naming/NamingModel',
        'Mixins/AsModelBoundView'
    ], function (Marionette, NamingModel, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Naming/NamingTemplate',

            initialize: function () {
                this.model = new NamingModel();
                this.model.fetch();
            }

        });

        return AsModelBoundView.call(view);
    });
