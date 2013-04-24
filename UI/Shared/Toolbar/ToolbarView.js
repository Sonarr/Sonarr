"use strict";
define(['app', 'Shared/Toolbar/CommandCollection'], function () {

    NzbDrone.Shared.Toolbar.ToolbarView = Backbone.Marionette.ItemView.extend({
        template: 'Shared/Toolbar/ToolbarTemplate',

        initialize: function () {
            if (!this.collection) {
                throw 'CommandCollection needs to be provided';
            }

            this.model = new Backbone.Model();
            this.model.set('commands', this.collection.toJSON());

        }
    });
});




