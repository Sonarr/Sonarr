'use strict';

var oldItemViewRender = Marionette.ItemView.prototype.render;
var oldItemCollectionViewRender = Marionette.CollectionView.prototype.render;

Marionette.ItemView.prototype.render = function () {

    if (this.model) {
        NzbDrone.ModelBinder.bind(this.model, this.el);
    }

    console.log("render");

    return oldItemViewRender.apply(this, arguments);
};

Marionette.CollectionView.prototype.render = function () {

    if (this.model) {
        NzbDrone.ModelBinder.bind(this.model, this.el);
    }

    console.log("render");

    return oldItemCollectionViewRender.apply(this, arguments);
};
