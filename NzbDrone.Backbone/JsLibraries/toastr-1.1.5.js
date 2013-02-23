/*
 * Copyright 2012 John Papa and Hans Fjällemark.  
 * All Rights Reserved.  
 * Use, reproduction, distribution, and modification of this code is subject to the terms and 
 * conditions of the MIT license, available at http://www.opensource.org/licenses/mit-license.php
 *
 * Author: John Papa and Hans Fjällemark
 * Project: https://github.com/CodeSeven/toastr
 */
; (function (define) {
    define(['jquery'], function ($) {
        var toastr = (function () {
            var
                defaults = {
                    tapToDismiss: true,
                    toastClass: 'toast',
                    containerId: 'toast-container',
                    debug: false,
                    fadeIn: 300,
                    fadeOut: 1000,
                    extendedTimeOut: 1000,
                    iconClasses: {
                        error: 'toast-error',
                        info: 'toast-info',
                        success: 'toast-success',
                        warning: 'toast-warning'
                    },
                    iconClass: 'toast-info',
                    positionClass: 'toast-top-right',
                    timeOut: 5000, // Set timeOut to 0 to make it sticky
                    titleClass: 'toast-title',
                    messageClass: 'toast-message',
                    target: $('body')
                },

                error = function (message, title, optionsOverride) {
                    return notify({
                        iconClass: getOptions().iconClasses.error,
                        message: message,
                        optionsOverride: optionsOverride,
                        title: title
                    });
                },

                getContainer = function (options) {
                    var $container = $('#' + options.containerId);
                    if ($container.length) {
                        return $container;
                    }
                    $container = $('<div/>')
                        .attr('id', options.containerId)
                        .addClass(options.positionClass);
                    $container.appendTo($(options.target));
                    return $container;
                },

                getOptions = function () {
                    return $.extend({}, defaults, toastr.options);
                },

                info = function (message, title, optionsOverride) {
                    return notify({
                        iconClass: getOptions().iconClasses.info,
                        message: message,
                        optionsOverride: optionsOverride,
                        title: title
                    });
                },

                notify = function (map) {
                    var
                        options = getOptions(),
                        iconClass = map.iconClass || options.iconClass;

                    if (typeof (map.optionsOverride) !== 'undefined') {
                        options = $.extend(options, map.optionsOverride);
                        iconClass = map.optionsOverride.iconClass || iconClass;
                    }

                    var
                        intervalId = null,
                        $container = getContainer(options),
                        $toastElement = $('<div/>'),
                        $titleElement = $('<div/>'),
                        $messageElement = $('<div/>'),
                        response = { options: options, map: map };

                    if (map.iconClass) {
                        $toastElement.addClass(options.toastClass).addClass(iconClass);
                    }

                    if (map.title) {
                        $titleElement.append(map.title).addClass(options.titleClass);
                        $toastElement.append($titleElement);
                    }

                    if (map.message) {
                        $messageElement.append(map.message).addClass(options.messageClass);
                        $toastElement.append($messageElement);
                    }

                    var fadeAway = function () {
                        if ($(':focus', $toastElement).length > 0) {
                            return;
                        }
                        var fade = function (callback) {
                            return $toastElement.fadeOut(options.fadeOut, callback);
                        };
                        var removeToast = function () {
                            if ($toastElement.is(':visible')) {
                                return;
                            }
                            $toastElement.remove();
                            if ($container.children().length === 0) {
                                $container.remove();
                            }
                        };
                        fade(removeToast);
                    };
                    var delayedFadeAway = function () {
                        if (options.timeOut > 0 || options.extendedTimeOut > 0) {
                            intervalId = setTimeout(fadeAway, options.extendedTimeOut);
                        }
                    };
                    var stickAround = function () {
                        clearTimeout(intervalId);
                        $toastElement.stop(true, true).fadeIn(options.fadeIn);
                    };
                    $toastElement.hide();
                    $container.prepend($toastElement);
                    $toastElement.fadeIn(options.fadeIn);
                    if (options.timeOut > 0) {
                        intervalId = setTimeout(fadeAway, options.timeOut);
                    }

                    $toastElement.hover(stickAround, delayedFadeAway);
                    if (!options.onclick && options.tapToDismiss) {
                        $toastElement.click(fadeAway);
                    }

                    if (options.onclick) {
                        $toastElement.click(function () {
                            options.onclick() && fadeAway();
                        });
                    }

                    if (options.debug && console) {
                        console.log(response);
                    }
                    return $toastElement;
                },

                success = function (message, title, optionsOverride) {
                    return notify({
                        iconClass: getOptions().iconClasses.success,
                        message: message,
                        optionsOverride: optionsOverride,
                        title: title
                    });
                },

                warning = function (message, title, optionsOverride) {
                    return notify({
                        iconClass: getOptions().iconClasses.warning,
                        message: message,
                        optionsOverride: optionsOverride,
                        title: title
                    });
                },

                clear = function ($toastElement) {
                    var options = getOptions();
                    var $container = getContainer();
                    if ($toastElement && $(':focus', $toastElement).length === 0) {
                        var removeToast = function () {
                            if ($toastElement.is(':visible')) {
                                return;
                            }
                            $toastElement.remove();
                            if ($container.children().length === 0) {
                                $container.remove();
                            }
                        };
                        $toastElement.fadeOut(options.fadeOut, removeToast);
                        return;
                    }
                    if ($container.length) {
                        $container.fadeOut(options.fadeOut, function () {
                            $container.remove();
                        });
                    }
                };
            return {
                clear: clear,
                error: error,
                info: info,
                options: {},
                success: success,
                version: '1.1.5',
                warning: warning
            };
        })();
        return toastr;
    });
}(typeof define === 'function' && define.amd ? define : function (deps, factory) {
    if (typeof module !== 'undefined' && module.exports) { //Node
        module.exports = factory(require(deps[0]));
    } else {
        window['toastr'] = factory(window['jQuery']);
    }
}));
