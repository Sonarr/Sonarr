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

module.exports = {
    appInitializer : function() {

        $('body').tooltip({ selector : '[title]' });

        return this;
    }
};