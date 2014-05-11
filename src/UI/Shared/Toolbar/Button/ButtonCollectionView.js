'use strict';
define(
    [
        'marionette',
        'Shared/Toolbar/Button/ButtonView'
    ], function (Marionette, ButtonView) {
        return Marionette.CollectionView.extend({
            className : 'btn-group',
            itemView  : ButtonView,

            initialize: function (options) {
                this.menu = options.menu;
                this.className = 'btn-group';

                if (options.menu.collapse) {
                    this.className += ' btn-group-collapse';
                }
            },

            onRender: function () {
                if (this.menu.collapse) {
                    this.$el.addClass('btn-group-collapse');
                }
            }
        });
    });


