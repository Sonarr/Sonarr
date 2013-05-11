"use strict";
define(['app', 'Shared/Toolbar/Button/ButtonView', 'Config'], function () {
    NzbDrone.Shared.Toolbar.ButtonCollectionView = Backbone.Marionette.CollectionView.extend({
        className: 'btn-group',
        itemView : NzbDrone.Shared.Toolbar.ButtonView
    });
});


