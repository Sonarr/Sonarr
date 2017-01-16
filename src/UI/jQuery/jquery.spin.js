module.exports = function() {
    'use strict';

    var $ = this;

    $.fn.spinForPromise = function(promise) {
        var self = this;

        if (!promise || promise.state() !== 'pending') {
            return this;
        }
        promise.always(function() {
            self.stopSpin();
        });

        return this.startSpin();
    };

    $.fn.startSpin = function() {
        var icon = this.find('i').andSelf('i');

        if (!icon || !icon.attr('class')) {
            return this;
        }

        var iconClasses = icon.attr('class').match(/(?:^|\s)icon\-.+?(?:$|\s)/);

        if (!iconClasses || iconClasses.length === 0) {
            return this;
        }

        var iconClass = $.trim(iconClasses[0]);

        this.addClass('disabled');

        if (icon.hasClass('icon-can-spin')) {
            icon.addClass('fa-spin');
        } else {
            icon.attr('data-idle-icon', iconClass);
            icon.removeClass(iconClass);
            icon.addClass('fa-spin-overlay');
            icon.html('<i class="' + iconClass + '" /><span><i class="fa-spin icon-sonarr-spinner" /></span>');
        }

        return this;
    };

    $.fn.stopSpin = function() {
        var icon = this.find('i').andSelf('i');

        icon.empty();
        this.removeClass('disabled');
        icon.removeClass('fa-spin fa-spin-overlay');
        var idleIcon = icon.attr('data-idle-icon');

        if (idleIcon) {
            icon.addClass(idleIcon);
        }

        return this;
    };
};