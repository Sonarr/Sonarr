var $ = require('jquery');
require('bootstrap');

var Tooltip = $.fn.tooltip.Constructor;

var origGetOptions = Tooltip.prototype.getOptions;
Tooltip.prototype.getOptions = function(options) {
    var result = origGetOptions.call(this, options);

    if (result.container === false) {

        var container = this.$element.closest('.btn-group,.input-group').parent();

        if (container.length) {
            result.container = container;
        }
    }

    return result;
};

var onElementRemoved = function(event) {
    event.data.hide();
};

var origShow = Tooltip.prototype.show;
Tooltip.prototype.show = function() {
    origShow.call(this);

    this.$element.on('remove', this, onElementRemoved);
};

var origHide = Tooltip.prototype.hide;
Tooltip.prototype.hide = function() {
    origHide.call(this);

    this.$element.off('remove', onElementRemoved);
};

module.exports = {
    appInitializer : function() {

        $('body').tooltip({ selector : '[title]' });

        return this;
    }
};