'use strict';

var oldItemViewRender = Marionette.ItemView.prototype.render;
var oldItemCollectionViewRender = Marionette.CollectionView.prototype.render;


Marionette.View.prototype.viewName = function () {
    if (this.template) {
        var regex = new RegExp('\/', 'g');

        return this.template
            .toLocaleLowerCase()
            .replace('template','')
            .replace(regex, '-');
    }

    return undefined;
};

Marionette.ItemView.prototype.render = function () {

    var result = oldItemViewRender.apply(this, arguments);

    this.$el.addClass('iv-' + this.viewName());

    if (this.model) {
        NzbDrone.ModelBinder.bind(this.model, this.el);
    }

    return result;
};

Marionette.CollectionView.prototype.render = function () {

    if (this.model) {
        NzbDrone.ModelBinder.bind(this.model, this.el);
    }

    return oldItemCollectionViewRender.apply(this, arguments);
};