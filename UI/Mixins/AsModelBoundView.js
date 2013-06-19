'use strict';

define(
    ['backbone.modelbinder'],
    function (ModelBinder) {

        return function () {

            var originalOnRender = this.prototype.onRender,
                originalBeforeClose = this.prototype.onBeforeClose;

            this.prototype.onRender = function () {

                if (!this.model) {
                    throw 'View has no model for binding';
                }

                if (!this._modelBinder) {
                    this._modelBinder = new ModelBinder();
                }

                this._modelBinder.bind(this.model, this.el);

                if (originalOnRender) {
                    originalOnRender.call(this);
                }
            };

            this.prototype.beforeClose = function () {

                if (this._modelBinder) {
                    this._modelBinder.unbind();
                    delete this._modelBinder;
                }

                if (originalBeforeClose) {
                    originalBeforeClose.call(this);
                }
            };

            return this;
        };
    }
);
