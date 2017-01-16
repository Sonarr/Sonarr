var Validation = require('backbone.validation');
var _ = require('underscore');

module.exports = (function() {
    'use strict';
    return function() {

        var originalInitialize = this.prototype.initialize;
        var originalOnRender = this.prototype.onRender;
        var originalBeforeClose = this.prototype.onBeforeClose;

        var errorHandler = function(response) {
            if (this.model) {
                this.model.trigger('validation:failed', response);
            } else {
                this.trigger('validation:failed', response);
            }
        };

        var validatedSync = function(method, model, options) {
            model.trigger('validation:sync');

            arguments[2].isValidatedCall = true;
            return model._originalSync.apply(this, arguments).fail(errorHandler.bind(this));
        };

        var bindToModel = function(model) {
            if (!model._originalSync) {
                model._originalSync = model.sync;
                model.sync = validatedSync.bind(this);
            }
        };

        var validationFailed = function(response) {
            if (response.status === 400) {
                var view = this;
                var validationErrors = JSON.parse(response.responseText);
                _.each(validationErrors, function(error) {
                    view.$el.processServerError(error);
                });
            }
        };

        this.prototype.initialize = function(options) {
            if (this.model) {
                this.listenTo(this.model, 'validation:sync', function() {
                    this.$el.removeAllErrors();
                });

                this.listenTo(this.model, 'validation:failed', validationFailed);
            } else {
                this.listenTo(this, 'validation:sync', function() {
                    this.$el.removeAllErrors();
                });

                this.listenTo(this, 'validation:failed', validationFailed);
            }

            if (originalInitialize) {
                originalInitialize.call(this, options);
            }
        };

        this.prototype.onRender = function() {
            Validation.bind(this);
            this.bindToModelValidation = bindToModel.bind(this);

            if (this.model) {
                this.bindToModelValidation(this.model);
            }

            if (originalOnRender) {
                originalOnRender.call(this);
            }
        };

        this.prototype.onBeforeClose = function() {
            if (this.model) {
                Validation.unbind(this);

                //If we don't do this the next time the model is used the sync is bound to an old view
                this.model.sync = this.model._originalSync;
                this.model._originalSync = undefined;
            }

            if (originalBeforeClose) {
                originalBeforeClose.call(this);
            }
        };

        return this;
    };
}).call(this);