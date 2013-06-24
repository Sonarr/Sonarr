'use strict';
define(['app', 'Shared/Toolbar/Button/ButtonView', 'Config'], function (App, ButtonView, Config) {
    return Backbone.Marionette.CollectionView.extend({
        className: 'btn-group',
        itemView : ButtonView
    });
});


