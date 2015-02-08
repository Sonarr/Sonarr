var ModelBinder = require('backbone.modelbinder');

module.exports = function() {

    var originalOnRender = this.prototype.onRender;
    var originalBeforeClose = this.prototype.onBeforeClose;

    this.prototype.onRender = function() {

        if (!this.model) {
            throw 'View has no model for binding';
        }

        if (!this._modelBinder) {
            this._modelBinder = new ModelBinder();
        }

        var options = {
            changeTriggers : {
                ''                  : 'change typeahead:selected typeahead:autocompleted',
                '[contenteditable]' : 'blur',
                '[data-onkeyup]'    : 'keyup'
            }
        };

        this._modelBinder.bind(this.model, this.el, null, options);

        if (originalOnRender) {
            originalOnRender.call(this);
        }
    };

    this.prototype.onBeforeClose = function() {

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
