define(
    [
        'jquery'
    ], function ($) {
        'use strict';

        $.fn.processServerError = function (error) {

            var validationName = error.propertyName.toLowerCase();

            this.find('.validation-errors')
                .addClass('alert alert-danger')
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

            var controlGroup = input.parents('.form-group');

            if(controlGroup.length === 0) {
                controlGroup = input.parent();
            }
            else{
                var inputGroup = controlGroup.find('.input-group');

                if (inputGroup.length === 0) {
                    controlGroup.append('<span class="help-inline error-message">' + error.errorMessage + '</span>');
                }

                else {
                    inputGroup.parent().append('<span class="help-block error-message">' + error.errorMessage + '</span>');
                }
            }

            controlGroup.addClass('has-error');

            return controlGroup.find('.help-inline').text();
        };


        $.fn.processClientError = function (error) {

        };

        $.fn.addFormError = function (error) {
            var t1 = this.find('.form-horizontal');
            var t2 = this.find('.form-horizontal').parent();

            this.prepend('<div class="alert alert-danger validation-error">' + error.errorMessage + '</div>');
        };

        $.fn.removeAllErrors = function () {
            this.find('.error').removeClass('error');
            this.find('.validation-errors').removeClass('alert').removeClass('alert-danger').html('');
            this.find('.validation-error').remove();
            return this.find('.help-inline.error-message').remove();
        };

    });
