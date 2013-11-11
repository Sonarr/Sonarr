'use strict';
define(
    [
        'backbone',
        'marionette',
        'underscore',
        'Commands/CommandController'
    ], function (Backbone, Marionette, _, CommandController) {

        return Marionette.ItemView.extend({
            template : 'Shared/Toolbar/ButtonTemplate',
            className: 'btn',

            ui: {
                icon: 'i'
            },

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

                if (!this.model.get('title')) {
                    this.$el.addClass('btn-icon-only');
                }

                var command = this.model.get('command');
                if (command) {
                    var properties = _.extend({ name: command }, this.model.get('properties'));

                    CommandController.bindToCommand({
                        command: properties,
                        element: this.$el
                    });
                }
            },

            onClick: function () {

                if (this.$el.hasClass('disabled')) {
                    return;
                }

                this.invokeCallback();
                this.invokeRoute();
                this.invokeCommand();
            },

            invokeCommand: function () {
                var command = this.model.get('command');
                if (command) {
                    CommandController.Execute(command, this.model.get('properties'));
                }
            },

            invokeRoute: function () {
                var route = this.model.get('route');
                if (route) {
                    Backbone.history.navigate(route, {trigger: true});
                }
            },

            invokeCallback: function () {

                if (!this.model.ownerContext) {
                    throw 'ownerContext must be set.';
                }

                var callback = this.model.get('callback');
                if (callback) {
                    callback.call(this.model.ownerContext, this);
                }
            }
        });
    });




