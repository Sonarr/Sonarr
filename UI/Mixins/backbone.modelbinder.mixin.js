'use strict';

var oldMarionetteItemViewRender = Marionette.ItemView.prototype.render;
var oldItemCollectionViewRender = Marionette.CollectionView.prototype.render;


Marionette.View.prototype.viewName = function () {
    if (this.template) {
        var regex = new RegExp('\/', 'g');

        return this.template
            .toLocaleLowerCase()
            .replace('template', '')
            .replace(regex, '-');
    }

    return undefined;
};

Marionette.ItemView.prototype.self$ = function (selector) {
    return  this.$(selector).not("[class*='iv-'] " + selector);
};


Marionette.ItemView.prototype._handleRelativeLink = function (event) {
    console.log('clikc');
    event.preventDefault();
    var $target = $(event.target);

    var href = event.target.getAttribute('href');

    if (!href && $target.parent('a') && $target.parent('a')[0]) {

        var linkElement = $target.parent('a')[0];

        href = linkElement.getAttribute('href');
    }

    if (!href) {
        throw 'couldnt find route target';
    }

    NzbDrone.Router.navigate(href, { trigger: true });
};


Marionette.ItemView.prototype.render = function () {

    var result = oldMarionetteItemViewRender.apply(this, arguments);

    this.$el.removeClass('iv-' + this.viewName());

    //check to see if el has bindings (name attribute)
    // any element that has a name attribute and isn't child of another view.
    if (this.self$('[name]').length > 0) {
        if (!this.model) {
            throw 'view ' + this.viewName() + ' has binding attributes but model is not defined';
        }

        if (!this._modelBinder) {
            this._modelBinder = new Backbone.ModelBinder();
        }

        this._modelBinder.bind(this.model, this.el);
    }

    this.$('a[href^="/"]').children().click(this._handleRelativeLink);
    this.$el.addClass('iv-' + this.viewName());

    return result;
};
