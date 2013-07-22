'use strict';
define(['Commands/CommandController', 'Shared/Messenger'],
    function(CommandController, Messenger) {
        return {
            ExecuteCommand: function (options) {
                options.iconClass = this._getIconClass(options.element);

                this._setSpinnerOnElement(options);

                var promise = CommandController.Execute(options.command, options.properties);
                this._handlePromise(promise, options);
            },

            SaveModel: function (options) {
                options.iconClass = this._getIconClass(options.element);

                this._setSpinnerOnElement(options);
                var promise = options.context.model.save();

                this._handlePromise(promise, options);
            },

            _handlePromise: function (promise, options) {
                promise.done(function () {
                    if (options.successMessage) {
                        Messenger.show({
                            message: options.successMessage
                        });
                    }

                    if (options.succesCallback) {
                        options.successCallback.call(options.context);
                    }
                });

                promise.fail(function (ajaxOptions) {
                    if (ajaxOptions.readyState === 0 || ajaxOptions.status === 0) {
                        return;
                    }

                    if (options.failMessage) {
                        Messenger.show({
                            message: options.failMessage,
                            type   : 'error'
                        });
                    }

                    if (options.failCallback) {
                        options.failCallback.call(options.context);
                    }
                });

                promise.always(function () {

                    if (options.leaveIcon) {
                        options.element.removeClass('icon-spin');
                    }

                    else {
                        options.element.addClass(options.iconClass);
                        options.element.removeClass('icon-nd-spinner');
                    }

                    if (options.alwaysCallback) {
                        options.alwaysCallback.call(options.context);
                    }
                });
            },

            _getIconClass: function(element) {
                return element.attr('class').match(/(?:^|\s)icon\-.+?(?:$|\s)/)[0];
            },

            _setSpinnerOnElement: function (options) {
                if (options.leaveIcon) {
                    options.element.addClass('icon-spin');
                }

                else {
                    options.element.removeClass(options.iconClass);
                    options.element.addClass('icon-nd-spinner');
                }
            }
        }
});
