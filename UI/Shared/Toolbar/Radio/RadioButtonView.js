'use strict';
define(
    [
        'marionette',
        'Config'
    ], function (Marionette, Config) {

        return Marionette.ItemView.extend({
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

                Config.SetValue(this.model.get('menuKey'), this.model.get('key'));
                this.invokeCallback();
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




