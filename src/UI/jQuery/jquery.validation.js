define(
    [
        'jquery'
    ], function ($) {
        'use strict';

        $.fn.processServerError = function (error) {

            var validationName = error.propertyName.toLowerCase();

            this.find('.validation-errors')
                .addClass('alert alert-error')
                .append('<div><i class="icon-exclamation-sign"></i>' + error.errorMessage + '</div>');

            var input = this.find('[name]').filter(function () {
                return this.name.toLowerCase() === validationName;
            });


            if (input.length === 0) {
                input = this.find('[validation-name]').filter(function () {
                    return $(this).attr('validation-name').toLowerCase() === validationName;
                });

                //still not found?
                if (input.length === 0) {
                    this.addFormError(error);
                    console.error('couldn\'t find input for ' + error.propertyName);
                    return this;
                }
            }

            var controlGroup = input.parents('.control-group');

            if(controlGroup.length ===0){
                controlGroup = input.parent();
            }
            else{
                controlGroup.find('.controls').append('<span class="help-inline error-message">' + error.errorMessage + '</span>');
            }

            controlGroup.addClass('error');

            return controlGroup.find('.help-inline').text();
        };


        $.fn.processClientError = function (error) {

        };

        $.fn.addFormError = function (error) {
            this.find('.control-group').parent().prepend('<div class="alert alert-error validation-error">' + error.errorMessage + '</div>');
        };

        $.fn.removeAllErrors = function () {
            this.find('.error').removeClass('error');
            this.find('.validation-errors').removeClass('alert').removeClass('alert-error').html('');
            this.find('.validation-error').remove();
            return this.find('.help-inline.error-message').remove();
        };

    });
