'use strict';
define(
    [
        'app',
        'marionette',
        'Shared/Actioneer',
        'Shared/Messenger'
    ], function (App, Marionette, Actioneer, Messenger) {

        return Marionette.ItemView.extend({
            template : 'Shared/Toolbar/ButtonTemplate',
            className: 'btn',

            events: {
                'click': 'onClick'
            },

            ui: {
                icon: '.x-icon'
            },

            initialize: function () {
                this.storageKey = this.model.get('menuKey') + ':' + this.model.get('key');
                this.idle = true;
            },

            onRender: function () {
                if (this.model.get('active')) {
                    this.$el.addClass('active');
                    this.invokeCallback();
                }

                if(!this.model.get('title')){
                    this.$el.addClass('btn-icon-only');
                }
            },

            onClick: function () {
                if (this.idle) {
                    this.invokeCallback();
                    this.invokeRoute();
                    this.invokeCommand();
                }
            },

            invokeCommand: function () {
                var command = this.model.get('command');
                if (command) {
                    this.idle = false;

                    Actioneer.ExecuteCommand({
                        command       : command,
                        button        : this.$el,
                        element       : this.ui.icon,
                        errorMessage  : this.model.get('errorMessage'),
                        successMessage: this.model.get('successMessage'),
                        always        : this._commandAlways,
                        context       : this
                    });
                }
            },

            invokeRoute: function () {
                var route = this.model.get('route');
                if (route) {

                    require(
                        [
                            'Router'
                        ], function () {
                            App.Router.navigate(route, {trigger: true});
                        });
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
            },

            _commandAlways: function () {
                if (!this.isClosed) {
                    this.idle = true;
                }
            }
        });
    });




