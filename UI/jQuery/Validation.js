define(
    [
        'jquery'
    ], function ($) {
        'use strict';

        $.fn.addBootstrapError = function (error) {
            var input = this.find('[name]').filter(function () {
                return this.name.toLowerCase() === error.propertyName.toLowerCase();
            });

            var controlGroup = input.parents('.control-group');
            if (controlGroup.find('.help-inline').length === 0) {
                controlGroup.find('.controls').append('<span class="help-inline error-message">' + error.errorMessage + '</span>');
            }

            controlGroup.addClass('error');

            return controlGroup.find('.help-inline').text();
        };


        $.fn.removeBootstrapError = function () {

            this.removeClass('error');

            return this.parents('.control-group').find('.help-inline.error-message').remove();
        };

    });
