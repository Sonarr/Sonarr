"use strict";
define(['app'], function () {

    NzbDrone.Shared.Toolbar.CommandView = Backbone.Marionette.ItemView.extend({
        template : 'Shared/Toolbar/CommandTemplate',
        className: 'btn',

        events: {
            'click': 'onClick'
        },

        onClick: function () {
            window.alert('click');
        }

    });
});




