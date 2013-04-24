"use strict";
define(['app', 'Shared/Toolbar/ButtonGroupView','Shared/Toolbar/CommandCollection'], function () {
    NzbDrone.Shared.Toolbar.ToolbarLayout = Backbone.Marionette.Layout.extend({
        template: 'Shared/Toolbar/ToolbarLayoutTemplate',

        regions: {
            left_1 : '.x-toolbar-left-1',
            left_2 : '.x-toolbar-left-2',
            right_1: '.x-toolbar-right-1',
            right_2: '.x-toolbar-right-2'
        },

        initialize: function (options) {
            this.left = options.left;
            this.right = options.right;
        },

        onShow: function () {
            if (this.left) {
                _.each(this.left, this._showToolbarLeft, this);
            }
            if (this.right) {
                _.each(this.right, this._showToolbarRight, this);
            }
        },

        _showToolbarLeft: function (element, index) {
            this['left_' + (index + 1).toString()].show(new NzbDrone.Shared.Toolbar.ButtonGroupView({collection: element}));
        },

        _showToolbarRight: function (element, index) {
            this['right_' + (index + 1).toString()].show(new NzbDrone.Shared.Toolbar.ButtonGroupView({collection: element}));
        }
    });

});




