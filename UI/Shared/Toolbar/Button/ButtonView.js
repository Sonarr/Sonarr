"use strict";
define(['app', 'Config'], function () {

    NzbDrone.Shared.Toolbar.ButtonView = Backbone.Marionette.ItemView.extend({
        template : 'Shared/Toolbar/ButtonTemplate',
        className: 'btn',

        events: {
            'click': 'onClick'
        },


        initialize: function () {
            this.storageKey = this.model.get('menuKey') + ':' + this.model.get('key');
        },

        onRender: function () {
            if (this.model.get('active')) {
                this.$el.addClass('active');
                this.invokeCallback();
            }
        },

        onClick: function () {
            this.invokeRoute();
            this.invokeCallback();
            this.invokeCommand();
        },


        invokeCommand: function () {
            var command = this.model.get('command');
            if (command) {
                window.alert(command);
            }
        },

        invokeRoute: function () {
            var route = this.model.get('route');
            if (route) {
                NzbDrone.Router.navigate(route, {trigger: true});
            }
        },

        invokeCallback: function () {

            if (!this.model.ownerContext) {
                throw 'ownerContext must be set.';
            }


            var callback = this.model.get('callback');
            if (callback) {
                callback.call(this.model.ownerContext);
            }
        }

    });
});




