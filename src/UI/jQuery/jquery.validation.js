module.exports = function() {
    'use strict';
    var $ = this;
    $.fn.processServerError = function(error) {
        var validationName = error.propertyName.toLowerCase();

        var errorMessage = this.formatErrorMessage(error);

        this.find('.validation-errors').addClass('alert alert-danger').append('<div><i class="icon-sonarr-form-danger"></i>' + errorMessage + '</div>');

        if (!validationName || validationName === '') {
            this.addFormError(error);
            return this;
        }

        var input = this.find('[name]').filter(function() {
            return this.name.toLowerCase() === validationName;
        });

        if (input.length === 0) {
            input = this.find('[validation-name]').filter(function() {
                return $(this).attr('validation-name').toLowerCase() === validationName;
            });

            //still not found?
            if (input.length === 0) {
                this.addFormError(error);
                console.error('couldn\'t find input for ' + error.propertyName);
                return this;
            }
        }

        var formGroup = input.parents('.form-group');

        if (formGroup.length === 0) {
            formGroup = input.parent();
        } else {
            var inputGroup = formGroup.find('.input-group');

            var validationClass = error.isWarning ? 'validation-warning' : 'validation-error';

            if (inputGroup.length === 0) {
                formGroup.append('<span class="help-inline {0}">{1}</span>'.format(validationClass, errorMessage));
            }
            else {
                inputGroup.parent().append('<span class="help-block {0}">{1}</span>'.format(validationClass, errorMessage));
            }
        }

        if (error.isWarning) {
            formGroup.addClass('has-warning');
        } else {
            formGroup.addClass('has-error');
        }

        return formGroup.find('.help-inline').text();
    };

    $.fn.processClientError = function(error) {

    };

    $.fn.addFormError = function(error) {

        var errorMessage = this.formatErrorMessage(error);

        var target = this.find('.modal-body');
        if (!target.length) {
            target = this;
        }

        var validationClass = error.isWarning ? 'alert alert-warning validation-warning' : 'alert alert-danger validation-error';

        target.prepend('<div class="{0}">{1}</div>'.format(validationClass, errorMessage));
    };

    $.fn.removeAllErrors = function() {
        this.find('.has-error').removeClass('has-error');
        this.find('.has-warning').removeClass('has-warning');
        this.find('.error').removeClass('error');
        this.find('.validation-errors').removeClass('alert').removeClass('alert-danger').removeClass('alert-warning').html('');
        this.find('.validation-error').remove();
        this.find('.validation-warning').remove();
        return this.find('.help-inline.error-message').remove();
    };

    $.fn.formatErrorMessage = function(error) {

        var errorMessage = error.errorMessage;

        if (error.infoLink) {
            if (error.detailedDescription) {
                errorMessage += ' <a class="no-router" target="_blank" href="' + error.infoLink + '"><i class="icon-sonarr-external-link" title="' + error.detailedDescription + '"></i></a>';
            } else {
                errorMessage += ' <a class="no-router" target="_blank" href="' + error.infoLink + '"><i class="icon-sonarr-external-link"></i></a>';
            }
        } else if (error.detailedDescription) {
            errorMessage += ' <i class="icon-sonarr-form-info" title="' + error.detailedDescription + '"></i>';
        }

        return errorMessage;
    };
};