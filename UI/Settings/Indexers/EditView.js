'use strict';

define([
    'app',
    'marionette',
    'Mixins/AsModelBoundView'
], function (App, Marionette, AsModelBoundView) {

    var view = Marionette.ItemView.extend({
        template: 'Settings/Indexers/EditTemplate',

        events: {
            'click .x-save': 'save'
        },

        initialize: function (options) {
            this.indexerCollection = options.indexerCollection;
        },

        save: function () {
            this.model.saveSettings();
        }
    });

    return AsModelBoundView.call(view);
});
