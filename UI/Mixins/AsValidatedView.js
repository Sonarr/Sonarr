define(
    [
        'backbone.validation',
        'underscore',
        'jQuery/jquery.validation'
    ], function (Validation, _) {
        'use strict';

        return function () {

            var originalOnRender = this.prototype.onRender;
            var originalOnClose = this.prototype.onClose;
            var originalBeforeClose = this.prototype.onBeforeClose;

            var errorHandler = function (response) {

                if (response.status === 400) {

                    var view = this;
                    var validationErrors = JSON.parse(response.responseText);
                    _.each(validationErrors, function (error) {
                        view.$el.processServerError(error);
                    });
                }
            };


            var validatedSync = function (method, model,options) {
                this.$el.removeAllErrors();
                arguments[2].isValidatedCall = true;
                return model._originalSync.apply(this, arguments).fail(errorHandler.bind(this));
            };

            var bindToModel = function (model) {

                if (!model._originalSync) {
                    model._originalSync = model.sync;
                    model.sync = validatedSync.bind(this);
                }
            };

            this.prototype.onRender = function () {

                Validation.bind(this);
                this.bindToModelValidation = bindToModel.bind(this);

                if (this.model) {
                    this.bindToModelValidation(this.model);
                }

                if (originalOnRender) {
                    originalOnRender.call(this);
                }
            };


            this.prototype.onBeforeClose = function () {

                if (this.model) {
                    Validation.unbind(this);
                }

                if (originalBeforeClose) {
                    originalBeforeClose.call(this);
                }
            };

            this.prototype.onClose = function () {

                if (this.model && this.model.isNew()) {
                    this.model.destroy();
                }

                if (originalOnClose) {
                    originalBeforeClose.call(this);
                }
            };


            return this;
        };
    });
