define(
    [
        'jquery'
    ], function ($) {
        'use strict';

        $.fn.startSpin = function () {

            var icon = this.find('i');

            var iconClasses = icon.attr('class').match(/(?:^|\s)icon\-.+?(?:$|\s)/);

            if (iconClasses.length === 0) {
                return this;
            }

            var iconClass = $.trim(iconClasses[0]);

            this.addClass('disabled');

            if (icon.hasClass('icon-can-spin')) {
                icon.addClass('icon-spin');
            }
            else {
                icon.attr('data-idle-icon', iconClass);
                icon.removeClass(iconClass);
                icon.addClass('icon-nd-spinner');
            }

            return this;
        };

        $.fn.stopSpin = function () {
            var icon = this.find('i');

            this.removeClass('disabled');
            icon.removeClass('icon-spin icon-nd-spinner');
            var idleIcon = icon.attr('data-idle-icon');

            if (idleIcon) {
                icon.addClass(idleIcon);
            }
            return this;
        };
    });
