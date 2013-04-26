"use strict";
define(['app'], function () {

    NzbDrone.Shared.Toolbar.RadioButtonView = Backbone.Marionette.ItemView.extend({
        template : 'Shared/Toolbar/ButtonTemplate',
        className: 'btn',

        events: {
            'click': 'invokeCallback'
        },


        onRender: function () {
            if (this.model.get('active')) {
                this.$el.addClass('active');
                this.invokeCallback();
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




