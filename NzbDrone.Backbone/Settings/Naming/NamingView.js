'use strict';

define([
        'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.Naming.NamingView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Naming/NamingTemplate',

        events: {
            'click .x-save': 'save'
        },

        initialize: function (options) {
            this.model = options.model;
        },

        onRender: function () {
            NzbDrone.ModelBinder.bind(this.model, this.el);
        },


        save: function () {
            //Todo: Actually save the model
            alert('Save pressed!');
        }
    });
});
