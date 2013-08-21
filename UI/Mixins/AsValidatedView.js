define(
    [
        'backbone.validation',
        'underscore',
        'jQuery/Validation'
    ], function (Validation, _) {
        'use strict';

        return function () {

            var originalOnRender = this.prototype.onRender;
            var originalOnClose = this.prototype.onClose;
            var originalBeforeClose = this.prototype.onBeforeClose;


            this.prototype.onRender = function () {

                Validation.bind(this);


                if (!this.originalSync && this.model) {

                    var self = this;
                    this.originalSync = this.model.sync;


                    var boundHandler = errorHandler.bind(this);

                    this.model.sync = function () {
                        self.$el.removeBootstrapError();
                        return self.originalSync.apply(this, arguments).fail(boundHandler);
                    };
                }

                if (this.model) {
                    if (originalOnRender) {
                        originalOnRender.call(this);
                    }
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


            var errorHandler = function (response) {

                if (response.status === 400) {

                    var view = this;

                    var validationErrors = JSON.parse(response.responseText);

                    _.each(validationErrors, function (error) {
                        view.$el.addBootstrapError(error);
                    });
                }
            };
        };
    });
