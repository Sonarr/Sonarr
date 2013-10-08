﻿'use strict';
define(
    [
        'marionette',
        'Mixins/AsModelBoundView'
    ], function (Marionette, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Indexers/Options/IndexerOptionsViewTemplate'
        });

        return AsModelBoundView.call(view);
    });
