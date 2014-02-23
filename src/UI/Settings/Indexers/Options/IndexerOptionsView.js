﻿'use strict';
define(
    [
        'marionette',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView'
    ], function (Marionette, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Indexers/Options/IndexerOptionsViewTemplate'
        });

        AsModelBoundView.call(view);
        AsValidatedView.call(view);

        return view;
    });
