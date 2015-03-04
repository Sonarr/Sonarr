var Backgrid = require('backgrid');

module.exports = Backgrid.Row.extend({
    className : 'manual-import-row',

    _originalInit : Backgrid.Row.prototype.initialize,
    _originalRender : Backgrid.Row.prototype.render,

    initialize : function () {
        this._originalInit.apply(this, arguments);

        this.listenTo(this.model, 'change', this._setError);
    },

    render : function () {
        this._originalRender.apply(this, arguments);
        this._setError();

        return this;
    },

    _setError : function () {
        if (this.model.has('series') &&
            this.model.has('seasonNumber') &&
            (this.model.has('episodes') && this.model.get('episodes').length > 0)&&
            this.model.has('quality')) {
            this.$el.removeClass('manual-import-error');
        }

        else {
            this.$el.addClass('manual-import-error');
        }
    }
});