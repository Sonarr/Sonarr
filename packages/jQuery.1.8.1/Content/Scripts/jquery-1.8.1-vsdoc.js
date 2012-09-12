/*
* This file has been generated to support Visual Studio IntelliSense.
* You should not use this file at runtime inside the browser--it is only
* intended to be used only for design-time IntelliSense.  Please use the
* standard jQuery library for all production use.
*
* Comment version: 1.8.1
*/

/*!
* jQuery JavaScript Library v1.8.1
* http://jquery.com/
*
* Distributed in whole under the terms of the MIT
*
* Copyright 2010, John Resig
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be
* included in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
* NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
* LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
* OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
* WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
* Includes Sizzle.js
* http://sizzlejs.com/
* Copyright 2010, The Dojo Foundation
* Released under the MIT and BSD Licenses.
*/

(function (window, undefined) {
    var jQuery = function (selector, context) {
        /// <summary>
        ///     1: Accepts a string containing a CSS selector which is then used to match a set of elements.
        ///     &#10;    1.1 - $(selector, context) 
        ///     &#10;    1.2 - $(element) 
        ///     &#10;    1.3 - $(object) 
        ///     &#10;    1.4 - $(elementArray) 
        ///     &#10;    1.5 - $(jQuery object) 
        ///     &#10;    1.6 - $()
        ///     &#10;2: Creates DOM elements on the fly from the provided string of raw HTML.
        ///     &#10;    2.1 - $(html, ownerDocument) 
        ///     &#10;    2.2 - $(html, props)
        ///     &#10;3: Binds a function to be executed when the DOM has finished loading.
        ///     &#10;    3.1 - $(callback)
        /// </summary>
        /// <param name="selector" type="String">
        ///     A string containing a selector expression
        /// </param>
        /// <param name="context" type="jQuery">
        ///     A DOM Element, Document, or jQuery to use as context
        /// </param>
        /// <returns type="jQuery" />

        // The jQuery object is actually just the init constructor 'enhanced'
        return new jQuery.fn.init(selector, context, rootjQuery);
    };
    jQuery.Animation = function Animation(elem, properties, options) {

        var result,
            index = 0,
            tweenerIndex = 0,
            length = animationPrefilters.length,
            deferred = jQuery.Deferred().always(function () {
                // don't match elem in the :animated selector
                delete tick.elem;
            }),
            tick = function () {
                var currentTime = fxNow || createFxNow(),
                    remaining = Math.max(0, animation.startTime + animation.duration - currentTime),
                    percent = 1 - (remaining / animation.duration || 0),
                    index = 0,
                    length = animation.tweens.length;

                for (; index < length ; index++) {
                    animation.tweens[index].run(percent);
                }

                deferred.notifyWith(elem, [animation, percent, remaining]);

                if (percent < 1 && length) {
                    return remaining;
                } else {
                    deferred.resolveWith(elem, [animation]);
                    return false;
                }
            },
            animation = deferred.promise({
                elem: elem,
                props: jQuery.extend({}, properties),
                opts: jQuery.extend(true, { specialEasing: {} }, options),
                originalProperties: properties,
                originalOptions: options,
                startTime: fxNow || createFxNow(),
                duration: options.duration,
                tweens: [],
                createTween: function (prop, end, easing) {
                    var tween = jQuery.Tween(elem, animation.opts, prop, end,
                            animation.opts.specialEasing[prop] || animation.opts.easing);
                    animation.tweens.push(tween);
                    return tween;
                },
                stop: function (gotoEnd) {
                    var index = 0,
                        // if we are going to the end, we want to run all the tweens
                        // otherwise we skip this part
                        length = gotoEnd ? animation.tweens.length : 0;

                    for (; index < length ; index++) {
                        animation.tweens[index].run(1);
                    }

                    // resolve when we played the last frame
                    // otherwise, reject
                    if (gotoEnd) {
                        deferred.resolveWith(elem, [animation, gotoEnd]);
                    } else {
                        deferred.rejectWith(elem, [animation, gotoEnd]);
                    }
                    return this;
                }
            }),
            props = animation.props;

        propFilter(props, animation.opts.specialEasing);

        for (; index < length ; index++) {
            result = animationPrefilters[index].call(animation, elem, props, animation.opts);
            if (result) {
                return result;
            }
        }

        createTweens(animation, props);

        if (jQuery.isFunction(animation.opts.start)) {
            animation.opts.start.call(elem, animation);
        }

        jQuery.fx.timer(
            jQuery.extend(tick, {
                anim: animation,
                queue: animation.opts.queue,
                elem: elem
            })
        );

        // attach callbacks from options
        return animation.progress(animation.opts.progress)
            .done(animation.opts.done, animation.opts.complete)
            .fail(animation.opts.fail)
            .always(animation.opts.always);
    };
    jQuery.Callbacks = function (options) {
        /// <summary>
        ///     A multi-purpose callbacks list object that provides a powerful way to manage callback lists.
        /// </summary>
        /// <param name="options" type="String">
        ///     An optional list of space-separated flags that change how the callback list behaves.
        /// </param>


        // Convert options from String-formatted to Object-formatted if needed
        // (we check in cache first)
        options = typeof options === "string" ?
            (optionsCache[options] || createOptions(options)) :
            jQuery.extend({}, options);

        var // Last fire value (for non-forgettable lists)
            memory,
            // Flag to know if list was already fired
            fired,
            // Flag to know if list is currently firing
            firing,
            // First callback to fire (used internally by add and fireWith)
            firingStart,
            // End of the loop when firing
            firingLength,
            // Index of currently firing callback (modified by remove if needed)
            firingIndex,
            // Actual callback list
            list = [],
            // Stack of fire calls for repeatable lists
            stack = !options.once && [],
            // Fire callbacks
            fire = function (data) {
                memory = options.memory && data;
                fired = true;
                firingIndex = firingStart || 0;
                firingStart = 0;
                firingLength = list.length;
                firing = true;
                for (; list && firingIndex < firingLength; firingIndex++) {
                    if (list[firingIndex].apply(data[0], data[1]) === false && options.stopOnFalse) {
                        memory = false; // To prevent further calls using add
                        break;
                    }
                }
                firing = false;
                if (list) {
                    if (stack) {
                        if (stack.length) {
                            fire(stack.shift());
                        }
                    } else if (memory) {
                        list = [];
                    } else {
                        self.disable();
                    }
                }
            },
            // Actual Callbacks object
            self = {
                // Add a callback or a collection of callbacks to the list
                add: function () {
                    if (list) {
                        // First, we save the current length
                        var start = list.length;
                        (function add(args) {
                            jQuery.each(args, function (_, arg) {
                                var type = jQuery.type(arg);
                                if (type === "function" && (!options.unique || !self.has(arg))) {
                                    list.push(arg);
                                } else if (arg && arg.length && type !== "string") {
                                    // Inspect recursively
                                    add(arg);
                                }
                            });
                        })(arguments);
                        // Do we need to add the callbacks to the
                        // current firing batch?
                        if (firing) {
                            firingLength = list.length;
                            // With memory, if we're not firing then
                            // we should call right away
                        } else if (memory) {
                            firingStart = start;
                            fire(memory);
                        }
                    }
                    return this;
                },
                // Remove a callback from the list
                remove: function () {
                    if (list) {
                        jQuery.each(arguments, function (_, arg) {
                            var index;
                            while ((index = jQuery.inArray(arg, list, index)) > -1) {
                                list.splice(index, 1);
                                // Handle firing indexes
                                if (firing) {
                                    if (index <= firingLength) {
                                        firingLength--;
                                    }
                                    if (index <= firingIndex) {
                                        firingIndex--;
                                    }
                                }
                            }
                        });
                    }
                    return this;
                },
                // Control if a given callback is in the list
                has: function (fn) {
                    return jQuery.inArray(fn, list) > -1;
                },
                // Remove all callbacks from the list
                empty: function () {
                    list = [];
                    return this;
                },
                // Have the list do nothing anymore
                disable: function () {
                    list = stack = memory = undefined;
                    return this;
                },
                // Is it disabled?
                disabled: function () {
                    return !list;
                },
                // Lock the list in its current state
                lock: function () {
                    stack = undefined;
                    if (!memory) {
                        self.disable();
                    }
                    return this;
                },
                // Is it locked?
                locked: function () {
                    return !stack;
                },
                // Call all callbacks with the given context and arguments
                fireWith: function (context, args) {
                    args = args || [];
                    args = [context, args.slice ? args.slice() : args];
                    if (list && (!fired || stack)) {
                        if (firing) {
                            stack.push(args);
                        } else {
                            fire(args);
                        }
                    }
                    return this;
                },
                // Call all the callbacks with the given arguments
                fire: function () {
                    self.fireWith(this, arguments);
                    return this;
                },
                // To know if the callbacks have already been called at least once
                fired: function () {
                    return !!fired;
                }
            };

        return self;
    };
    jQuery.Deferred = function (func) {

        var tuples = [
                // action, add listener, listener list, final state
                ["resolve", "done", jQuery.Callbacks("once memory"), "resolved"],
                ["reject", "fail", jQuery.Callbacks("once memory"), "rejected"],
                ["notify", "progress", jQuery.Callbacks("memory")]
        ],
            state = "pending",
            promise = {
                state: function () {
                    return state;
                },
                always: function () {
                    deferred.done(arguments).fail(arguments);
                    return this;
                },
                then: function ( /* fnDone, fnFail, fnProgress */) {
                    var fns = arguments;
                    return jQuery.Deferred(function (newDefer) {
                        jQuery.each(tuples, function (i, tuple) {
                            var action = tuple[0],
                                fn = fns[i];
                            // deferred[ done | fail | progress ] for forwarding actions to newDefer
                            deferred[tuple[1]](jQuery.isFunction(fn) ?
                                function () {
                                    var returned = fn.apply(this, arguments);
                                    if (returned && jQuery.isFunction(returned.promise)) {
                                        returned.promise()
                                            .done(newDefer.resolve)
                                            .fail(newDefer.reject)
                                            .progress(newDefer.notify);
                                    } else {
                                        newDefer[action + "With"](this === deferred ? newDefer : this, [returned]);
                                    }
                                } :
                                newDefer[action]
                            );
                        });
                        fns = null;
                    }).promise();
                },
                // Get a promise for this deferred
                // If obj is provided, the promise aspect is added to the object
                promise: function (obj) {
                    return typeof obj === "object" ? jQuery.extend(obj, promise) : promise;
                }
            },
            deferred = {};

        // Keep pipe for back-compat
        promise.pipe = promise.then;

        // Add list-specific methods
        jQuery.each(tuples, function (i, tuple) {
            var list = tuple[2],
                stateString = tuple[3];

            // promise[ done | fail | progress ] = list.add
            promise[tuple[1]] = list.add;

            // Handle state
            if (stateString) {
                list.add(function () {
                    // state = [ resolved | rejected ]
                    state = stateString;

                    // [ reject_list | resolve_list ].disable; progress_list.lock
                }, tuples[i ^ 1][2].disable, tuples[2][2].lock);
            }

            // deferred[ resolve | reject | notify ] = list.fire
            deferred[tuple[0]] = list.fire;
            deferred[tuple[0] + "With"] = list.fireWith;
        });

        // Make the deferred a promise
        promise.promise(deferred);

        // Call given func if any
        if (func) {
            func.call(deferred, deferred);
        }

        // All done!
        return deferred;
    };
    jQuery.Event = function (src, props) {

        // Allow instantiation without the 'new' keyword
        if (!(this instanceof jQuery.Event)) {
            return new jQuery.Event(src, props);
        }

        // Event object
        if (src && src.type) {
            this.originalEvent = src;
            this.type = src.type;

            // Events bubbling up the document may have been marked as prevented
            // by a handler lower down the tree; reflect the correct value.
            this.isDefaultPrevented = (src.defaultPrevented || src.returnValue === false ||
                src.getPreventDefault && src.getPreventDefault()) ? returnTrue : returnFalse;

            // Event type
        } else {
            this.type = src;
        }

        // Put explicitly provided properties onto the event object
        if (props) {
            jQuery.extend(this, props);
        }

        // Create a timestamp if incoming event doesn't have one
        this.timeStamp = src && src.timeStamp || jQuery.now();

        // Mark it as fixed
        this[jQuery.expando] = true;
    };
    jQuery.Tween = function Tween(elem, options, prop, end, easing) {

        return new Tween.prototype.init(elem, options, prop, end, easing);
    };
    jQuery._data = function (elem, name, data) {

        return jQuery.data(elem, name, data, true);
    };
    jQuery._queueHooks = function (elem, type) {

        var key = type + "queueHooks";
        return jQuery._data(elem, key) || jQuery._data(elem, key, {
            empty: jQuery.Callbacks("once memory").add(function () {
                jQuery.removeData(elem, type + "queue", true);
                jQuery.removeData(elem, key, true);
            })
        });
    };
    jQuery.acceptData = function (elem) {

        var noData = elem.nodeName && jQuery.noData[elem.nodeName.toLowerCase()];

        // nodes accept data unless otherwise specified; rejection can be conditional
        return !noData || noData !== true && elem.getAttribute("classid") === noData;
    };
    jQuery.access = function (elems, fn, key, value, chainable, emptyGet, pass) {

        var exec,
            bulk = key == null,
            i = 0,
            length = elems.length;

        // Sets many values
        if (key && typeof key === "object") {
            for (i in key) {
                jQuery.access(elems, fn, i, key[i], 1, emptyGet, value);
            }
            chainable = 1;

            // Sets one value
        } else if (value !== undefined) {
            // Optionally, function values get executed if exec is true
            exec = pass === undefined && jQuery.isFunction(value);

            if (bulk) {
                // Bulk operations only iterate when executing function values
                if (exec) {
                    exec = fn;
                    fn = function (elem, key, value) {
                        return exec.call(jQuery(elem), value);
                    };

                    // Otherwise they run against the entire set
                } else {
                    fn.call(elems, value);
                    fn = null;
                }
            }

            if (fn) {
                for (; i < length; i++) {
                    fn(elems[i], key, exec ? value.call(elems[i], i, fn(elems[i], key)) : value, pass);
                }
            }

            chainable = 1;
        }

        return chainable ?
            elems :

            // Gets
            bulk ?
                fn.call(elems) :
                length ? fn(elems[0], key) : emptyGet;
    };
    jQuery.active = 0;
    jQuery.ajax = function (url, options) {
        /// <summary>
        ///     Perform an asynchronous HTTP (Ajax) request.
        ///     &#10;1 - jQuery.ajax(url, settings) 
        ///     &#10;2 - jQuery.ajax(settings)
        /// </summary>
        /// <param name="url" type="String">
        ///     A string containing the URL to which the request is sent.
        /// </param>
        /// <param name="options" type="Object">
        ///     A set of key/value pairs that configure the Ajax request. All settings are optional. A default can be set for any option with $.ajaxSetup(). See jQuery.ajax( settings ) below for a complete list of all settings.
        /// </param>


        // If url is an object, simulate pre-1.5 signature
        if (typeof url === "object") {
            options = url;
            url = undefined;
        }

        // Force options to be an object
        options = options || {};

        var // ifModified key
            ifModifiedKey,
            // Response headers
            responseHeadersString,
            responseHeaders,
            // transport
            transport,
            // timeout handle
            timeoutTimer,
            // Cross-domain detection vars
            parts,
            // To know if global events are to be dispatched
            fireGlobals,
            // Loop variable
            i,
            // Create the final options object
            s = jQuery.ajaxSetup({}, options),
            // Callbacks context
            callbackContext = s.context || s,
            // Context for global events
            // It's the callbackContext if one was provided in the options
            // and if it's a DOM node or a jQuery collection
            globalEventContext = callbackContext !== s &&
                (callbackContext.nodeType || callbackContext instanceof jQuery) ?
                        jQuery(callbackContext) : jQuery.event,
            // Deferreds
            deferred = jQuery.Deferred(),
            completeDeferred = jQuery.Callbacks("once memory"),
            // Status-dependent callbacks
            statusCode = s.statusCode || {},
            // Headers (they are sent all at once)
            requestHeaders = {},
            requestHeadersNames = {},
            // The jqXHR state
            state = 0,
            // Default abort message
            strAbort = "canceled",
            // Fake xhr
            jqXHR = {

                readyState: 0,

                // Caches the header
                setRequestHeader: function (name, value) {
                    if (!state) {
                        var lname = name.toLowerCase();
                        name = requestHeadersNames[lname] = requestHeadersNames[lname] || name;
                        requestHeaders[name] = value;
                    }
                    return this;
                },

                // Raw string
                getAllResponseHeaders: function () {
                    return state === 2 ? responseHeadersString : null;
                },

                // Builds headers hashtable if needed
                getResponseHeader: function (key) {
                    var match;
                    if (state === 2) {
                        if (!responseHeaders) {
                            responseHeaders = {};
                            while ((match = rheaders.exec(responseHeadersString))) {
                                responseHeaders[match[1].toLowerCase()] = match[2];
                            }
                        }
                        match = responseHeaders[key.toLowerCase()];
                    }
                    return match === undefined ? null : match;
                },

                // Overrides response content-type header
                overrideMimeType: function (type) {
                    if (!state) {
                        s.mimeType = type;
                    }
                    return this;
                },

                // Cancel the request
                abort: function (statusText) {
                    statusText = statusText || strAbort;
                    if (transport) {
                        transport.abort(statusText);
                    }
                    done(0, statusText);
                    return this;
                }
            };

        // Callback for when everything is done
        // It is defined here because jslint complains if it is declared
        // at the end of the function (which would be more logical and readable)
        function done(status, nativeStatusText, responses, headers) {
            var isSuccess, success, error, response, modified,
                statusText = nativeStatusText;

            // Called once
            if (state === 2) {
                return;
            }

            // State is "done" now
            state = 2;

            // Clear timeout if it exists
            if (timeoutTimer) {
                clearTimeout(timeoutTimer);
            }

            // Dereference transport for early garbage collection
            // (no matter how long the jqXHR object will be used)
            transport = undefined;

            // Cache response headers
            responseHeadersString = headers || "";

            // Set readyState
            jqXHR.readyState = status > 0 ? 4 : 0;

            // Get response data
            if (responses) {
                response = ajaxHandleResponses(s, jqXHR, responses);
            }

            // If successful, handle type chaining
            if (status >= 200 && status < 300 || status === 304) {

                // Set the If-Modified-Since and/or If-None-Match header, if in ifModified mode.
                if (s.ifModified) {

                    modified = jqXHR.getResponseHeader("Last-Modified");
                    if (modified) {
                        jQuery.lastModified[ifModifiedKey] = modified;
                    }
                    modified = jqXHR.getResponseHeader("Etag");
                    if (modified) {
                        jQuery.etag[ifModifiedKey] = modified;
                    }
                }

                // If not modified
                if (status === 304) {

                    statusText = "notmodified";
                    isSuccess = true;

                    // If we have data
                } else {

                    isSuccess = ajaxConvert(s, response);
                    statusText = isSuccess.state;
                    success = isSuccess.data;
                    error = isSuccess.error;
                    isSuccess = !error;
                }
            } else {
                // We extract error from statusText
                // then normalize statusText and status for non-aborts
                error = statusText;
                if (!statusText || status) {
                    statusText = "error";
                    if (status < 0) {
                        status = 0;
                    }
                }
            }

            // Set data for the fake xhr object
            jqXHR.status = status;
            jqXHR.statusText = "" + (nativeStatusText || statusText);

            // Success/Error
            if (isSuccess) {
                deferred.resolveWith(callbackContext, [success, statusText, jqXHR]);
            } else {
                deferred.rejectWith(callbackContext, [jqXHR, statusText, error]);
            }

            // Status-dependent callbacks
            jqXHR.statusCode(statusCode);
            statusCode = undefined;

            if (fireGlobals) {
                globalEventContext.trigger("ajax" + (isSuccess ? "Success" : "Error"),
                        [jqXHR, s, isSuccess ? success : error]);
            }

            // Complete
            completeDeferred.fireWith(callbackContext, [jqXHR, statusText]);

            if (fireGlobals) {
                globalEventContext.trigger("ajaxComplete", [jqXHR, s]);
                // Handle the global AJAX counter
                if (!(--jQuery.active)) {
                    jQuery.event.trigger("ajaxStop");
                }
            }
        }

        // Attach deferreds
        deferred.promise(jqXHR);
        jqXHR.success = jqXHR.done;
        jqXHR.error = jqXHR.fail;
        jqXHR.complete = completeDeferred.add;

        // Status-dependent callbacks
        jqXHR.statusCode = function (map) {
            if (map) {
                var tmp;
                if (state < 2) {
                    for (tmp in map) {
                        statusCode[tmp] = [statusCode[tmp], map[tmp]];
                    }
                } else {
                    tmp = map[jqXHR.status];
                    jqXHR.always(tmp);
                }
            }
            return this;
        };

        // Remove hash character (#7531: and string promotion)
        // Add protocol if not provided (#5866: IE7 issue with protocol-less urls)
        // We also use the url parameter if available
        s.url = ((url || s.url) + "").replace(rhash, "").replace(rprotocol, ajaxLocParts[1] + "//");

        // Extract dataTypes list
        s.dataTypes = jQuery.trim(s.dataType || "*").toLowerCase().split(core_rspace);

        // Determine if a cross-domain request is in order
        if (s.crossDomain == null) {
            parts = rurl.exec(s.url.toLowerCase());
            s.crossDomain = !!(parts &&
                (parts[1] != ajaxLocParts[1] || parts[2] != ajaxLocParts[2] ||
                    (parts[3] || (parts[1] === "http:" ? 80 : 443)) !=
                        (ajaxLocParts[3] || (ajaxLocParts[1] === "http:" ? 80 : 443)))
            );
        }

        // Convert data if not already a string
        if (s.data && s.processData && typeof s.data !== "string") {
            s.data = jQuery.param(s.data, s.traditional);
        }

        // Apply prefilters
        inspectPrefiltersOrTransports(prefilters, s, options, jqXHR);

        // If request was aborted inside a prefilter, stop there
        if (state === 2) {
            return jqXHR;
        }

        // We can fire global events as of now if asked to
        fireGlobals = s.global;

        // Uppercase the type
        s.type = s.type.toUpperCase();

        // Determine if request has content
        s.hasContent = !rnoContent.test(s.type);

        // Watch for a new set of requests
        if (fireGlobals && jQuery.active++ === 0) {
            jQuery.event.trigger("ajaxStart");
        }

        // More options handling for requests with no content
        if (!s.hasContent) {

            // If data is available, append data to url
            if (s.data) {
                s.url += (rquery.test(s.url) ? "&" : "?") + s.data;
                // #9682: remove data so that it's not used in an eventual retry
                delete s.data;
            }

            // Get ifModifiedKey before adding the anti-cache parameter
            ifModifiedKey = s.url;

            // Add anti-cache in url if needed
            if (s.cache === false) {

                var ts = jQuery.now(),
                    // try replacing _= if it is there
                    ret = s.url.replace(rts, "$1_=" + ts);

                // if nothing was replaced, add timestamp to the end
                s.url = ret + ((ret === s.url) ? (rquery.test(s.url) ? "&" : "?") + "_=" + ts : "");
            }
        }

        // Set the correct header, if data is being sent
        if (s.data && s.hasContent && s.contentType !== false || options.contentType) {
            jqXHR.setRequestHeader("Content-Type", s.contentType);
        }

        // Set the If-Modified-Since and/or If-None-Match header, if in ifModified mode.
        if (s.ifModified) {
            ifModifiedKey = ifModifiedKey || s.url;
            if (jQuery.lastModified[ifModifiedKey]) {
                jqXHR.setRequestHeader("If-Modified-Since", jQuery.lastModified[ifModifiedKey]);
            }
            if (jQuery.etag[ifModifiedKey]) {
                jqXHR.setRequestHeader("If-None-Match", jQuery.etag[ifModifiedKey]);
            }
        }

        // Set the Accepts header for the server, depending on the dataType
        jqXHR.setRequestHeader(
            "Accept",
            s.dataTypes[0] && s.accepts[s.dataTypes[0]] ?
                s.accepts[s.dataTypes[0]] + (s.dataTypes[0] !== "*" ? ", " + allTypes + "; q=0.01" : "") :
                s.accepts["*"]
        );

        // Check for headers option
        for (i in s.headers) {
            jqXHR.setRequestHeader(i, s.headers[i]);
        }

        // Allow custom headers/mimetypes and early abort
        if (s.beforeSend && (s.beforeSend.call(callbackContext, jqXHR, s) === false || state === 2)) {
            // Abort if not done already and return
            return jqXHR.abort();

        }

        // aborting is no longer a cancellation
        strAbort = "abort";

        // Install callbacks on deferreds
        for (i in { success: 1, error: 1, complete: 1 }) {
            jqXHR[i](s[i]);
        }

        // Get transport
        transport = inspectPrefiltersOrTransports(transports, s, options, jqXHR);

        // If no transport, we auto-abort
        if (!transport) {
            done(-1, "No Transport");
        } else {
            jqXHR.readyState = 1;
            // Send global event
            if (fireGlobals) {
                globalEventContext.trigger("ajaxSend", [jqXHR, s]);
            }
            // Timeout
            if (s.async && s.timeout > 0) {
                timeoutTimer = setTimeout(function () {
                    jqXHR.abort("timeout");
                }, s.timeout);
            }

            try {
                state = 1;
                transport.send(requestHeaders, done);
            } catch (e) {
                // Propagate exception as error if not done
                if (state < 2) {
                    done(-1, e);
                    // Simply rethrow otherwise
                } else {
                    throw e;
                }
            }
        }

        return jqXHR;
    };
    jQuery.ajaxPrefilter = function (dataTypeExpression, func) {
        /// <summary>
        ///     Handle custom Ajax options or modify existing options before each request is sent and before they are processed by $.ajax().
        /// </summary>
        /// <param name="dataTypeExpression" type="String">
        ///     An optional string containing one or more space-separated dataTypes
        /// </param>
        /// <param name="func" type="Function">
        ///     A handler to set default values for future Ajax requests.
        /// </param>
        /// <returns type="undefined" />


        if (typeof dataTypeExpression !== "string") {
            func = dataTypeExpression;
            dataTypeExpression = "*";
        }

        var dataType, list, placeBefore,
            dataTypes = dataTypeExpression.toLowerCase().split(core_rspace),
            i = 0,
            length = dataTypes.length;

        if (jQuery.isFunction(func)) {
            // For each dataType in the dataTypeExpression
            for (; i < length; i++) {
                dataType = dataTypes[i];
                // We control if we're asked to add before
                // any existing element
                placeBefore = /^\+/.test(dataType);
                if (placeBefore) {
                    dataType = dataType.substr(1) || "*";
                }
                list = structure[dataType] = structure[dataType] || [];
                // then we add to the structure accordingly
                list[placeBefore ? "unshift" : "push"](func);
            }
        }
    };
    jQuery.ajaxSettings = {
        "url": 'http://localhost:25812/',
        "isLocal": false,
        "global": true,
        "type": 'GET',
        "contentType": 'application/x-www-form-urlencoded; charset=UTF-8',
        "processData": true,
        "async": true,
        "accepts": {},
        "contents": {},
        "responseFields": {},
        "converters": {},
        "flatOptions": {},
        "jsonp": 'callback'
    };
    jQuery.ajaxSetup = function (target, settings) {
        /// <summary>
        ///     Set default values for future Ajax requests.
        /// </summary>
        /// <param name="target" type="Object">
        ///     A set of key/value pairs that configure the default Ajax request. All options are optional.
        /// </param>

        if (settings) {
            // Building a settings object
            ajaxExtend(target, jQuery.ajaxSettings);
        } else {
            // Extending ajaxSettings
            settings = target;
            target = jQuery.ajaxSettings;
        }
        ajaxExtend(target, settings);
        return target;
    };
    jQuery.ajaxTransport = function (dataTypeExpression, func) {


        if (typeof dataTypeExpression !== "string") {
            func = dataTypeExpression;
            dataTypeExpression = "*";
        }

        var dataType, list, placeBefore,
            dataTypes = dataTypeExpression.toLowerCase().split(core_rspace),
            i = 0,
            length = dataTypes.length;

        if (jQuery.isFunction(func)) {
            // For each dataType in the dataTypeExpression
            for (; i < length; i++) {
                dataType = dataTypes[i];
                // We control if we're asked to add before
                // any existing element
                placeBefore = /^\+/.test(dataType);
                if (placeBefore) {
                    dataType = dataType.substr(1) || "*";
                }
                list = structure[dataType] = structure[dataType] || [];
                // then we add to the structure accordingly
                list[placeBefore ? "unshift" : "push"](func);
            }
        }
    };
    jQuery.attr = function (elem, name, value, pass) {

        var ret, hooks, notxml,
            nType = elem.nodeType;

        // don't get/set attributes on text, comment and attribute nodes
        if (!elem || nType === 3 || nType === 8 || nType === 2) {
            return;
        }

        if (pass && jQuery.isFunction(jQuery.fn[name])) {
            return jQuery(elem)[name](value);
        }

        // Fallback to prop when attributes are not supported
        if (typeof elem.getAttribute === "undefined") {
            return jQuery.prop(elem, name, value);
        }

        notxml = nType !== 1 || !jQuery.isXMLDoc(elem);

        // All attributes are lowercase
        // Grab necessary hook if one is defined
        if (notxml) {
            name = name.toLowerCase();
            hooks = jQuery.attrHooks[name] || (rboolean.test(name) ? boolHook : nodeHook);
        }

        if (value !== undefined) {

            if (value === null) {
                jQuery.removeAttr(elem, name);
                return;

            } else if (hooks && "set" in hooks && notxml && (ret = hooks.set(elem, value, name)) !== undefined) {
                return ret;

            } else {
                elem.setAttribute(name, "" + value);
                return value;
            }

        } else if (hooks && "get" in hooks && notxml && (ret = hooks.get(elem, name)) !== null) {
            return ret;

        } else {

            ret = elem.getAttribute(name);

            // Non-existent attributes return null, we normalize to undefined
            return ret === null ?
                undefined :
                ret;
        }
    };
    jQuery.attrFn = {};
    jQuery.attrHooks = {
        "type": {},
        "value": {}
    };
    jQuery.browser = {
        "chrome": true,
        "version": '21.0.1180.83',
        "webkit": true
    };
    jQuery.buildFragment = function (args, context, scripts) {

        var fragment, cacheable, cachehit,
            first = args[0];

        // Set context from what may come in as undefined or a jQuery collection or a node
        // Updated to fix #12266 where accessing context[0] could throw an exception in IE9/10 &
        // also doubles as fix for #8950 where plain objects caused createDocumentFragment exception
        context = context || document;
        context = !context.nodeType && context[0] || context;
        context = context.ownerDocument || context;

        // Only cache "small" (1/2 KB) HTML strings that are associated with the main document
        // Cloning options loses the selected state, so don't cache them
        // IE 6 doesn't like it when you put <object> or <embed> elements in a fragment
        // Also, WebKit does not clone 'checked' attributes on cloneNode, so don't cache
        // Lastly, IE6,7,8 will not correctly reuse cached fragments that were created from unknown elems #10501
        if (args.length === 1 && typeof first === "string" && first.length < 512 && context === document &&
            first.charAt(0) === "<" && !rnocache.test(first) &&
            (jQuery.support.checkClone || !rchecked.test(first)) &&
            (jQuery.support.html5Clone || !rnoshimcache.test(first))) {

            // Mark cacheable and look for a hit
            cacheable = true;
            fragment = jQuery.fragments[first];
            cachehit = fragment !== undefined;
        }

        if (!fragment) {
            fragment = context.createDocumentFragment();
            jQuery.clean(args, context, fragment, scripts);

            // Update the cache, but only store false
            // unless this is a second parsing of the same content
            if (cacheable) {
                jQuery.fragments[first] = cachehit && fragment;
            }
        }

        return { fragment: fragment, cacheable: cacheable };
    };
    jQuery.cache = {};
    jQuery.camelCase = function (string) {

        return string.replace(rmsPrefix, "ms-").replace(rdashAlpha, fcamelCase);
    };
    jQuery.clean = function (elems, context, fragment, scripts) {

        var i, j, elem, tag, wrap, depth, div, hasBody, tbody, len, handleScript, jsTags,
            safe = context === document && safeFragment,
            ret = [];

        // Ensure that context is a document
        if (!context || typeof context.createDocumentFragment === "undefined") {
            context = document;
        }

        // Use the already-created safe fragment if context permits
        for (i = 0; (elem = elems[i]) != null; i++) {
            if (typeof elem === "number") {
                elem += "";
            }

            if (!elem) {
                continue;
            }

            // Convert html string into DOM nodes
            if (typeof elem === "string") {
                if (!rhtml.test(elem)) {
                    elem = context.createTextNode(elem);
                } else {
                    // Ensure a safe container in which to render the html
                    safe = safe || createSafeFragment(context);
                    div = context.createElement("div");
                    safe.appendChild(div);

                    // Fix "XHTML"-style tags in all browsers
                    elem = elem.replace(rxhtmlTag, "<$1></$2>");

                    // Go to html and back, then peel off extra wrappers
                    tag = (rtagName.exec(elem) || ["", ""])[1].toLowerCase();
                    wrap = wrapMap[tag] || wrapMap._default;
                    depth = wrap[0];
                    div.innerHTML = wrap[1] + elem + wrap[2];

                    // Move to the right depth
                    while (depth--) {
                        div = div.lastChild;
                    }

                    // Remove IE's autoinserted <tbody> from table fragments
                    if (!jQuery.support.tbody) {

                        // String was a <table>, *may* have spurious <tbody>
                        hasBody = rtbody.test(elem);
                        tbody = tag === "table" && !hasBody ?
                            div.firstChild && div.firstChild.childNodes :

                            // String was a bare <thead> or <tfoot>
                            wrap[1] === "<table>" && !hasBody ?
                                div.childNodes :
                                [];

                        for (j = tbody.length - 1; j >= 0 ; --j) {
                            if (jQuery.nodeName(tbody[j], "tbody") && !tbody[j].childNodes.length) {
                                tbody[j].parentNode.removeChild(tbody[j]);
                            }
                        }
                    }

                    // IE completely kills leading whitespace when innerHTML is used
                    if (!jQuery.support.leadingWhitespace && rleadingWhitespace.test(elem)) {
                        div.insertBefore(context.createTextNode(rleadingWhitespace.exec(elem)[0]), div.firstChild);
                    }

                    elem = div.childNodes;

                    // Take out of fragment container (we need a fresh div each time)
                    div.parentNode.removeChild(div);
                }
            }

            if (elem.nodeType) {
                ret.push(elem);
            } else {
                jQuery.merge(ret, elem);
            }
        }

        // Fix #11356: Clear elements from safeFragment
        if (div) {
            elem = div = safe = null;
        }

        // Reset defaultChecked for any radios and checkboxes
        // about to be appended to the DOM in IE 6/7 (#8060)
        if (!jQuery.support.appendChecked) {
            for (i = 0; (elem = ret[i]) != null; i++) {
                if (jQuery.nodeName(elem, "input")) {
                    fixDefaultChecked(elem);
                } else if (typeof elem.getElementsByTagName !== "undefined") {
                    jQuery.grep(elem.getElementsByTagName("input"), fixDefaultChecked);
                }
            }
        }

        // Append elements to a provided document fragment
        if (fragment) {
            // Special handling of each script element
            handleScript = function (elem) {
                // Check if we consider it executable
                if (!elem.type || rscriptType.test(elem.type)) {
                    // Detach the script and store it in the scripts array (if provided) or the fragment
                    // Return truthy to indicate that it has been handled
                    return scripts ?
                        scripts.push(elem.parentNode ? elem.parentNode.removeChild(elem) : elem) :
                        fragment.appendChild(elem);
                }
            };

            for (i = 0; (elem = ret[i]) != null; i++) {
                // Check if we're done after handling an executable script
                if (!(jQuery.nodeName(elem, "script") && handleScript(elem))) {
                    // Append to fragment and handle embedded scripts
                    fragment.appendChild(elem);
                    if (typeof elem.getElementsByTagName !== "undefined") {
                        // handleScript alters the DOM, so use jQuery.merge to ensure snapshot iteration
                        jsTags = jQuery.grep(jQuery.merge([], elem.getElementsByTagName("script")), handleScript);

                        // Splice the scripts into ret after their former ancestor and advance our index beyond them
                        ret.splice.apply(ret, [i + 1, 0].concat(jsTags));
                        i += jsTags.length;
                    }
                }
            }
        }

        return ret;
    };
    jQuery.cleanData = function (elems, /* internal */ acceptData) {

        var data, id, elem, type,
            i = 0,
            internalKey = jQuery.expando,
            cache = jQuery.cache,
            deleteExpando = jQuery.support.deleteExpando,
            special = jQuery.event.special;

        for (; (elem = elems[i]) != null; i++) {

            if (acceptData || jQuery.acceptData(elem)) {

                id = elem[internalKey];
                data = id && cache[id];

                if (data) {
                    if (data.events) {
                        for (type in data.events) {
                            if (special[type]) {
                                jQuery.event.remove(elem, type);

                                // This is a shortcut to avoid jQuery.event.remove's overhead
                            } else {
                                jQuery.removeEvent(elem, type, data.handle);
                            }
                        }
                    }

                    // Remove cache only if it was not already removed by jQuery.event.remove
                    if (cache[id]) {

                        delete cache[id];

                        // IE does not allow us to delete expando properties from nodes,
                        // nor does it have a removeAttribute function on Document nodes;
                        // we must handle all of these cases
                        if (deleteExpando) {
                            delete elem[internalKey];

                        } else if (elem.removeAttribute) {
                            elem.removeAttribute(internalKey);

                        } else {
                            elem[internalKey] = null;
                        }

                        jQuery.deletedIds.push(id);
                    }
                }
            }
        }
    };
    jQuery.clone = function (elem, dataAndEvents, deepDataAndEvents) {

        var srcElements,
            destElements,
            i,
            clone;

        if (jQuery.support.html5Clone || jQuery.isXMLDoc(elem) || !rnoshimcache.test("<" + elem.nodeName + ">")) {
            clone = elem.cloneNode(true);

            // IE<=8 does not properly clone detached, unknown element nodes
        } else {
            fragmentDiv.innerHTML = elem.outerHTML;
            fragmentDiv.removeChild(clone = fragmentDiv.firstChild);
        }

        if ((!jQuery.support.noCloneEvent || !jQuery.support.noCloneChecked) &&
                (elem.nodeType === 1 || elem.nodeType === 11) && !jQuery.isXMLDoc(elem)) {
            // IE copies events bound via attachEvent when using cloneNode.
            // Calling detachEvent on the clone will also remove the events
            // from the original. In order to get around this, we use some
            // proprietary methods to clear the events. Thanks to MooTools
            // guys for this hotness.

            cloneFixAttributes(elem, clone);

            // Using Sizzle here is crazy slow, so we use getElementsByTagName instead
            srcElements = getAll(elem);
            destElements = getAll(clone);

            // Weird iteration because IE will replace the length property
            // with an element if you are cloning the body and one of the
            // elements on the page has a name or id of "length"
            for (i = 0; srcElements[i]; ++i) {
                // Ensure that the destination node is not null; Fixes #9587
                if (destElements[i]) {
                    cloneFixAttributes(srcElements[i], destElements[i]);
                }
            }
        }

        // Copy the events from the original to the clone
        if (dataAndEvents) {
            cloneCopyEvent(elem, clone);

            if (deepDataAndEvents) {
                srcElements = getAll(elem);
                destElements = getAll(clone);

                for (i = 0; srcElements[i]; ++i) {
                    cloneCopyEvent(srcElements[i], destElements[i]);
                }
            }
        }

        srcElements = destElements = null;

        // Return the cloned set
        return clone;
    };
    jQuery.contains = function (a, b) {
        /// <summary>
        ///     Check to see if a DOM element is within another DOM element.
        /// </summary>
        /// <param name="a" domElement="true">
        ///     The DOM element that may contain the other element.
        /// </param>
        /// <param name="b" domElement="true">
        ///     The DOM element that may be contained by the other element.
        /// </param>
        /// <returns type="Boolean" />

        var adown = a.nodeType === 9 ? a.documentElement : a,
            bup = b && b.parentNode;
        return a === bup || !!(bup && bup.nodeType === 1 && adown.contains && adown.contains(bup));
    };
    jQuery.css = function (elem, name, numeric, extra) {

        var val, num, hooks,
            origName = jQuery.camelCase(name);

        // Make sure that we're working with the right name
        name = jQuery.cssProps[origName] || (jQuery.cssProps[origName] = vendorPropName(elem.style, origName));

        // gets hook for the prefixed version
        // followed by the unprefixed version
        hooks = jQuery.cssHooks[name] || jQuery.cssHooks[origName];

        // If a hook was provided get the computed value from there
        if (hooks && "get" in hooks) {
            val = hooks.get(elem, true, extra);
        }

        // Otherwise, if a way to get the computed value exists, use that
        if (val === undefined) {
            val = curCSS(elem, name);
        }

        //convert "normal" to computed value
        if (val === "normal" && name in cssNormalTransform) {
            val = cssNormalTransform[name];
        }

        // Return, converting to number if forced or a qualifier was provided and val looks numeric
        if (numeric || extra !== undefined) {
            num = parseFloat(val);
            return numeric || jQuery.isNumeric(num) ? num || 0 : val;
        }
        return val;
    };
    jQuery.cssHooks = {
        "opacity": {},
        "height": {},
        "width": {},
        "margin": {},
        "padding": {},
        "borderWidth": {},
        "top": {},
        "left": {}
    };
    jQuery.cssNumber = {
        "fillOpacity": true,
        "fontWeight": true,
        "lineHeight": true,
        "opacity": true,
        "orphans": true,
        "widows": true,
        "zIndex": true,
        "zoom": true
    };
    jQuery.cssProps = {
        "float": 'cssFloat',
        "display": 'display',
        "visibility": 'visibility',
        "opacity": 'opacity'
    };
    jQuery.data = function (elem, name, data, pvt /* Internal Use Only */) {
        /// <summary>
        ///     1: Store arbitrary data associated with the specified element. Returns the value that was set.
        ///     &#10;    1.1 - jQuery.data(element, key, value)
        ///     &#10;2: Returns value at named data store for the element, as set by jQuery.data(element, name, value), or the full data store for the element.
        ///     &#10;    2.1 - jQuery.data(element, key) 
        ///     &#10;    2.2 - jQuery.data(element)
        /// </summary>
        /// <param name="elem" domElement="true">
        ///     The DOM element to associate with the data.
        /// </param>
        /// <param name="name" type="String">
        ///     A string naming the piece of data to set.
        /// </param>
        /// <param name="data" type="Object">
        ///     The new data value.
        /// </param>
        /// <returns type="Object" />

        if (!jQuery.acceptData(elem)) {
            return;
        }

        var thisCache, ret,
            internalKey = jQuery.expando,
            getByName = typeof name === "string",

            // We have to handle DOM nodes and JS objects differently because IE6-7
            // can't GC object references properly across the DOM-JS boundary
            isNode = elem.nodeType,

            // Only DOM nodes need the global jQuery cache; JS object data is
            // attached directly to the object so GC can occur automatically
            cache = isNode ? jQuery.cache : elem,

            // Only defining an ID for JS objects if its cache already exists allows
            // the code to shortcut on the same path as a DOM node with no cache
            id = isNode ? elem[internalKey] : elem[internalKey] && internalKey;

        // Avoid doing any more work than we need to when trying to get data on an
        // object that has no data at all
        if ((!id || !cache[id] || (!pvt && !cache[id].data)) && getByName && data === undefined) {
            return;
        }

        if (!id) {
            // Only DOM nodes need a new unique ID for each element since their data
            // ends up in the global cache
            if (isNode) {
                elem[internalKey] = id = jQuery.deletedIds.pop() || ++jQuery.uuid;
            } else {
                id = internalKey;
            }
        }

        if (!cache[id]) {
            cache[id] = {};

            // Avoids exposing jQuery metadata on plain JS objects when the object
            // is serialized using JSON.stringify
            if (!isNode) {
                cache[id].toJSON = jQuery.noop;
            }
        }

        // An object can be passed to jQuery.data instead of a key/value pair; this gets
        // shallow copied over onto the existing cache
        if (typeof name === "object" || typeof name === "function") {
            if (pvt) {
                cache[id] = jQuery.extend(cache[id], name);
            } else {
                cache[id].data = jQuery.extend(cache[id].data, name);
            }
        }

        thisCache = cache[id];

        // jQuery data() is stored in a separate object inside the object's internal data
        // cache in order to avoid key collisions between internal data and user-defined
        // data.
        if (!pvt) {
            if (!thisCache.data) {
                thisCache.data = {};
            }

            thisCache = thisCache.data;
        }

        if (data !== undefined) {
            thisCache[jQuery.camelCase(name)] = data;
        }

        // Check for both converted-to-camel and non-converted data property names
        // If a data property was specified
        if (getByName) {

            // First Try to find as-is property data
            ret = thisCache[name];

            // Test for null|undefined property data
            if (ret == null) {

                // Try to find the camelCased property
                ret = thisCache[jQuery.camelCase(name)];
            }
        } else {
            ret = thisCache;
        }

        return ret;
    };
    jQuery.dequeue = function (elem, type) {
        /// <summary>
        ///     Execute the next function on the queue for the matched element.
        /// </summary>
        /// <param name="elem" domElement="true">
        ///     A DOM element from which to remove and execute a queued function.
        /// </param>
        /// <param name="type" type="String">
        ///     A string containing the name of the queue. Defaults to fx, the standard effects queue.
        /// </param>
        /// <returns type="undefined" />

        type = type || "fx";

        var queue = jQuery.queue(elem, type),
            startLength = queue.length,
            fn = queue.shift(),
            hooks = jQuery._queueHooks(elem, type),
            next = function () {
                jQuery.dequeue(elem, type);
            };

        // If the fx queue is dequeued, always remove the progress sentinel
        if (fn === "inprogress") {
            fn = queue.shift();
            startLength--;
        }

        if (fn) {

            // Add a progress sentinel to prevent the fx queue from being
            // automatically dequeued
            if (type === "fx") {
                queue.unshift("inprogress");
            }

            // clear up the last queue stop function
            delete hooks.stop;
            fn.call(elem, next, hooks);
        }

        if (!startLength && hooks) {
            hooks.empty.fire();
        }
    };
    jQuery.dir = function (elem, dir, until) {

        var matched = [],
            cur = elem[dir];

        while (cur && cur.nodeType !== 9 && (until === undefined || cur.nodeType !== 1 || !jQuery(cur).is(until))) {
            if (cur.nodeType === 1) {
                matched.push(cur);
            }
            cur = cur[dir];
        }
        return matched;
    };
    jQuery.each = function (obj, callback, args) {
        /// <summary>
        ///     A generic iterator function, which can be used to seamlessly iterate over both objects and arrays. Arrays and array-like objects with a length property (such as a function's arguments object) are iterated by numeric index, from 0 to length-1. Other objects are iterated via their named properties.
        /// </summary>
        /// <param name="obj" type="Object">
        ///     The object or array to iterate over.
        /// </param>
        /// <param name="callback" type="Function">
        ///     The function that will be executed on every object.
        /// </param>
        /// <returns type="Object" />

        var name,
            i = 0,
            length = obj.length,
            isObj = length === undefined || jQuery.isFunction(obj);

        if (args) {
            if (isObj) {
                for (name in obj) {
                    if (callback.apply(obj[name], args) === false) {
                        break;
                    }
                }
            } else {
                for (; i < length;) {
                    if (callback.apply(obj[i++], args) === false) {
                        break;
                    }
                }
            }

            // A special, fast, case for the most common use of each
        } else {
            if (isObj) {
                for (name in obj) {
                    if (callback.call(obj[name], name, obj[name]) === false) {
                        break;
                    }
                }
            } else {
                for (; i < length;) {
                    if (callback.call(obj[i], i, obj[i++]) === false) {
                        break;
                    }
                }
            }
        }

        return obj;
    };
    jQuery.easing = {};
    jQuery.error = function (msg) {
        /// <summary>
        ///     Takes a string and throws an exception containing it.
        /// </summary>
        /// <param name="msg" type="String">
        ///     The message to send out.
        /// </param>

        throw new Error(msg);
    };
    jQuery.etag = {};
    jQuery.event = {
        "global": {},
        "customEvent": {},
        "props": ['attrChange', 'attrName', 'relatedNode', 'srcElement', 'altKey', 'bubbles', 'cancelable', 'ctrlKey', 'currentTarget', 'eventPhase', 'metaKey', 'relatedTarget', 'shiftKey', 'target', 'timeStamp', 'view', 'which'],
        "fixHooks": {},
        "keyHooks": {},
        "mouseHooks": {},
        "special": {},
        "triggered": {}
    };
    jQuery.expr = {
        "cacheLength": 50,
        "match": {},
        "order": {},
        "attrHandle": {},
        "find": {},
        "relative": {},
        "preFilter": {},
        "filter": {},
        "pseudos": {},
        "setFilters": {},
        "filters": {},
        ":": {}
    };
    jQuery.extend = function () {
        /// <summary>
        ///     Merge the contents of two or more objects together into the first object.
        ///     &#10;1 - jQuery.extend(target, object1, objectN) 
        ///     &#10;2 - jQuery.extend(deep, target, object1, objectN)
        /// </summary>
        /// <param name="" type="Boolean">
        ///     If true, the merge becomes recursive (aka. deep copy).
        /// </param>
        /// <param name="" type="Object">
        ///     The object to extend. It will receive the new properties.
        /// </param>
        /// <param name="" type="Object">
        ///     An object containing additional properties to merge in.
        /// </param>
        /// <param name="" type="Object">
        ///     Additional objects containing properties to merge in.
        /// </param>
        /// <returns type="Object" />

        var options, name, src, copy, copyIsArray, clone,
            target = arguments[0] || {},
            i = 1,
            length = arguments.length,
            deep = false;

        // Handle a deep copy situation
        if (typeof target === "boolean") {
            deep = target;
            target = arguments[1] || {};
            // skip the boolean and the target
            i = 2;
        }

        // Handle case when target is a string or something (possible in deep copy)
        if (typeof target !== "object" && !jQuery.isFunction(target)) {
            target = {};
        }

        // extend jQuery itself if only one argument is passed
        if (length === i) {
            target = this;
            --i;
        }

        for (; i < length; i++) {
            // Only deal with non-null/undefined values
            if ((options = arguments[i]) != null) {
                // Extend the base object
                for (name in options) {
                    src = target[name];
                    copy = options[name];

                    // Prevent never-ending loop
                    if (target === copy) {
                        continue;
                    }

                    // Recurse if we're merging plain objects or arrays
                    if (deep && copy && (jQuery.isPlainObject(copy) || (copyIsArray = jQuery.isArray(copy)))) {
                        if (copyIsArray) {
                            copyIsArray = false;
                            clone = src && jQuery.isArray(src) ? src : [];

                        } else {
                            clone = src && jQuery.isPlainObject(src) ? src : {};
                        }

                        // Never move original objects, clone them
                        target[name] = jQuery.extend(deep, clone, copy);

                        // Don't bring in undefined values
                    } else if (copy !== undefined) {
                        target[name] = copy;
                    }
                }
            }
        }

        // Return the modified object
        return target;
    };
    jQuery.filter = function (expr, elems, not) {

        if (not) {
            expr = ":not(" + expr + ")";
        }

        return elems.length === 1 ?
            jQuery.find.matchesSelector(elems[0], expr) ? [elems[0]] : [] :
            jQuery.find.matches(expr, elems);
    };
    jQuery.find = function Sizzle(selector, context, results, seed) {

        results = results || [];
        context = context || document;
        var match, elem, xml, m,
            nodeType = context.nodeType;

        if (nodeType !== 1 && nodeType !== 9) {
            return [];
        }

        if (!selector || typeof selector !== "string") {
            return results;
        }

        xml = isXML(context);

        if (!xml && !seed) {
            if ((match = rquickExpr.exec(selector))) {
                // Speed-up: Sizzle("#ID")
                if ((m = match[1])) {
                    if (nodeType === 9) {
                        elem = context.getElementById(m);
                        // Check parentNode to catch when Blackberry 4.6 returns
                        // nodes that are no longer in the document #6963
                        if (elem && elem.parentNode) {
                            // Handle the case where IE, Opera, and Webkit return items
                            // by name instead of ID
                            if (elem.id === m) {
                                results.push(elem);
                                return results;
                            }
                        } else {
                            return results;
                        }
                    } else {
                        // Context is not a document
                        if (context.ownerDocument && (elem = context.ownerDocument.getElementById(m)) &&
                            contains(context, elem) && elem.id === m) {
                            results.push(elem);
                            return results;
                        }
                    }

                    // Speed-up: Sizzle("TAG")
                } else if (match[2]) {
                    push.apply(results, slice.call(context.getElementsByTagName(selector), 0));
                    return results;

                    // Speed-up: Sizzle(".CLASS")
                } else if ((m = match[3]) && assertUsableClassName && context.getElementsByClassName) {
                    push.apply(results, slice.call(context.getElementsByClassName(m), 0));
                    return results;
                }
            }
        }

        // All others
        return select(selector, context, results, seed, xml);
    };
    jQuery.fn = {
        "selector": '',
        "jquery": '1.8.1',
        "length": 0
    };
    jQuery.fragments = {};
    jQuery.fx = function (elem, options, prop, end, easing, unit) {

        this.elem = elem;
        this.prop = prop;
        this.easing = easing || "swing";
        this.options = options;
        this.start = this.now = this.cur();
        this.end = end;
        this.unit = unit || (jQuery.cssNumber[prop] ? "" : "px");
    };
    jQuery.get = function (url, data, callback, type) {
        /// <summary>
        ///     Load data from the server using a HTTP GET request.
        /// </summary>
        /// <param name="url" type="String">
        ///     A string containing the URL to which the request is sent.
        /// </param>
        /// <param name="data" type="String">
        ///     A map or string that is sent to the server with the request.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A callback function that is executed if the request succeeds.
        /// </param>
        /// <param name="type" type="String">
        ///     The type of data expected from the server. Default: Intelligent Guess (xml, json, script, or html).
        /// </param>

        // shift arguments if data argument was omitted
        if (jQuery.isFunction(data)) {
            type = type || callback;
            callback = data;
            data = undefined;
        }

        return jQuery.ajax({
            type: method,
            url: url,
            data: data,
            success: callback,
            dataType: type
        });
    };
    jQuery.getJSON = function (url, data, callback) {
        /// <summary>
        ///     Load JSON-encoded data from the server using a GET HTTP request.
        /// </summary>
        /// <param name="url" type="String">
        ///     A string containing the URL to which the request is sent.
        /// </param>
        /// <param name="data" type="Object">
        ///     A map or string that is sent to the server with the request.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A callback function that is executed if the request succeeds.
        /// </param>

        return jQuery.get(url, data, callback, "json");
    };
    jQuery.getScript = function (url, callback) {
        /// <summary>
        ///     Load a JavaScript file from the server using a GET HTTP request, then execute it.
        /// </summary>
        /// <param name="url" type="String">
        ///     A string containing the URL to which the request is sent.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A callback function that is executed if the request succeeds.
        /// </param>

        return jQuery.get(url, undefined, callback, "script");
    };
    jQuery.globalEval = function (data) {
        /// <summary>
        ///     Execute some JavaScript code globally.
        /// </summary>
        /// <param name="data" type="String">
        ///     The JavaScript code to execute.
        /// </param>

        if (data && core_rnotwhite.test(data)) {
            // We use execScript on Internet Explorer
            // We use an anonymous function so that context is window
            // rather than jQuery in Firefox
            (window.execScript || function (data) {
                window["eval"].call(window, data);
            })(data);
        }
    };
    jQuery.grep = function (elems, callback, inv) {
        /// <summary>
        ///     Finds the elements of an array which satisfy a filter function. The original array is not affected.
        /// </summary>
        /// <param name="elems" type="Array">
        ///     The array to search through.
        /// </param>
        /// <param name="callback" type="Function">
        ///     The function to process each item against.  The first argument to the function is the item, and the second argument is the index.  The function should return a Boolean value.  this will be the global window object.
        /// </param>
        /// <param name="inv" type="Boolean">
        ///     If "invert" is false, or not provided, then the function returns an array consisting of all elements for which "callback" returns true.  If "invert" is true, then the function returns an array consisting of all elements for which "callback" returns false.
        /// </param>
        /// <returns type="Array" />

        var retVal,
            ret = [],
            i = 0,
            length = elems.length;
        inv = !!inv;

        // Go through the array, only saving the items
        // that pass the validator function
        for (; i < length; i++) {
            retVal = !!callback(elems[i], i);
            if (inv !== retVal) {
                ret.push(elems[i]);
            }
        }

        return ret;
    };
    jQuery.guid = 1;
    jQuery.hasData = function (elem) {
        /// <summary>
        ///     Determine whether an element has any jQuery data associated with it.
        /// </summary>
        /// <param name="elem" domElement="true">
        ///     A DOM element to be checked for data.
        /// </param>
        /// <returns type="Boolean" />

        elem = elem.nodeType ? jQuery.cache[elem[jQuery.expando]] : elem[jQuery.expando];
        return !!elem && !isEmptyDataObject(elem);
    };
    jQuery.holdReady = function (hold) {
        /// <summary>
        ///     Holds or releases the execution of jQuery's ready event.
        /// </summary>
        /// <param name="hold" type="Boolean">
        ///     Indicates whether the ready hold is being requested or released
        /// </param>
        /// <returns type="undefined" />

        if (hold) {
            jQuery.readyWait++;
        } else {
            jQuery.ready(true);
        }
    };
    jQuery.inArray = function (elem, arr, i) {
        /// <summary>
        ///     Search for a specified value within an array and return its index (or -1 if not found).
        /// </summary>
        /// <param name="elem" type="Object">
        ///     The value to search for.
        /// </param>
        /// <param name="arr" type="Array">
        ///     An array through which to search.
        /// </param>
        /// <param name="i" type="Number">
        ///     The index of the array at which to begin the search. The default is 0, which will search the whole array.
        /// </param>
        /// <returns type="Number" />

        var len;

        if (arr) {
            if (core_indexOf) {
                return core_indexOf.call(arr, elem, i);
            }

            len = arr.length;
            i = i ? i < 0 ? Math.max(0, len + i) : i : 0;

            for (; i < len; i++) {
                // Skip accessing in sparse arrays
                if (i in arr && arr[i] === elem) {
                    return i;
                }
            }
        }

        return -1;
    };
    jQuery.isEmptyObject = function (obj) {
        /// <summary>
        ///     Check to see if an object is empty (contains no properties).
        /// </summary>
        /// <param name="obj" type="Object">
        ///     The object that will be checked to see if it's empty.
        /// </param>
        /// <returns type="Boolean" />

        var name;
        for (name in obj) {
            return false;
        }
        return true;
    };
    jQuery.isFunction = function (obj) {
        /// <summary>
        ///     Determine if the argument passed is a Javascript function object.
        /// </summary>
        /// <param name="obj" type="Object">
        ///     Object to test whether or not it is a function.
        /// </param>
        /// <returns type="boolean" />

        return jQuery.type(obj) === "function";
    };
    jQuery.isNumeric = function (obj) {
        /// <summary>
        ///     Determines whether its argument is a number.
        /// </summary>
        /// <param name="obj" type="Object">
        ///     The value to be tested.
        /// </param>
        /// <returns type="Boolean" />

        return !isNaN(parseFloat(obj)) && isFinite(obj);
    };
    jQuery.isPlainObject = function (obj) {
        /// <summary>
        ///     Check to see if an object is a plain object (created using "{}" or "new Object").
        /// </summary>
        /// <param name="obj" type="Object">
        ///     The object that will be checked to see if it's a plain object.
        /// </param>
        /// <returns type="Boolean" />

        // Must be an Object.
        // Because of IE, we also have to check the presence of the constructor property.
        // Make sure that DOM nodes and window objects don't pass through, as well
        if (!obj || jQuery.type(obj) !== "object" || obj.nodeType || jQuery.isWindow(obj)) {
            return false;
        }

        try {
            // Not own constructor property must be Object
            if (obj.constructor &&
                !core_hasOwn.call(obj, "constructor") &&
                !core_hasOwn.call(obj.constructor.prototype, "isPrototypeOf")) {
                return false;
            }
        } catch (e) {
            // IE8,9 Will throw exceptions on certain host objects #9897
            return false;
        }

        // Own properties are enumerated firstly, so to speed up,
        // if last one is own, then all properties are own.

        var key;
        for (key in obj) { }

        return key === undefined || core_hasOwn.call(obj, key);
    };
    jQuery.isReady = true;
    jQuery.isWindow = function (obj) {
        /// <summary>
        ///     Determine whether the argument is a window.
        /// </summary>
        /// <param name="obj" type="Object">
        ///     Object to test whether or not it is a window.
        /// </param>
        /// <returns type="boolean" />

        return obj != null && obj == obj.window;
    };
    jQuery.isXMLDoc = function isXML(elem) {
        /// <summary>
        ///     Check to see if a DOM node is within an XML document (or is an XML document).
        /// </summary>
        /// <returns type="Boolean" />

        // documentElement is verified for cases where it doesn't yet exist
        // (such as loading iframes in IE - #4833)
        var documentElement = elem && (elem.ownerDocument || elem).documentElement;
        return documentElement ? documentElement.nodeName !== "HTML" : false;
    };
    jQuery.lastModified = {};
    jQuery.makeArray = function (arr, results) {
        /// <summary>
        ///     Convert an array-like object into a true JavaScript array.
        /// </summary>
        /// <param name="arr" type="Object">
        ///     Any object to turn into a native Array.
        /// </param>
        /// <returns type="Array" />

        var type,
            ret = results || [];

        if (arr != null) {
            // The window, strings (and functions) also have 'length'
            // Tweaked logic slightly to handle Blackberry 4.7 RegExp issues #6930
            type = jQuery.type(arr);

            if (arr.length == null || type === "string" || type === "function" || type === "regexp" || jQuery.isWindow(arr)) {
                core_push.call(ret, arr);
            } else {
                jQuery.merge(ret, arr);
            }
        }

        return ret;
    };
    jQuery.map = function (elems, callback, arg) {
        /// <summary>
        ///     Translate all items in an array or object to new array of items.
        ///     &#10;1 - jQuery.map(array, callback(elementOfArray, indexInArray)) 
        ///     &#10;2 - jQuery.map(arrayOrObject, callback( value, indexOrKey ))
        /// </summary>
        /// <param name="elems" type="Array">
        ///     The Array to translate.
        /// </param>
        /// <param name="callback" type="Function">
        ///     The function to process each item against.  The first argument to the function is the array item, the second argument is the index in array The function can return any value. Within the function, this refers to the global (window) object.
        /// </param>
        /// <returns type="Array" />

        var value, key,
            ret = [],
            i = 0,
            length = elems.length,
            // jquery objects are treated as arrays
            isArray = elems instanceof jQuery || length !== undefined && typeof length === "number" && ((length > 0 && elems[0] && elems[length - 1]) || length === 0 || jQuery.isArray(elems));

        // Go through the array, translating each of the items to their
        if (isArray) {
            for (; i < length; i++) {
                value = callback(elems[i], i, arg);

                if (value != null) {
                    ret[ret.length] = value;
                }
            }

            // Go through every key on the object,
        } else {
            for (key in elems) {
                value = callback(elems[key], key, arg);

                if (value != null) {
                    ret[ret.length] = value;
                }
            }
        }

        // Flatten any nested arrays
        return ret.concat.apply([], ret);
    };
    jQuery.merge = function (first, second) {
        /// <summary>
        ///     Merge the contents of two arrays together into the first array.
        /// </summary>
        /// <param name="first" type="Array">
        ///     The first array to merge, the elements of second added.
        /// </param>
        /// <param name="second" type="Array">
        ///     The second array to merge into the first, unaltered.
        /// </param>
        /// <returns type="Array" />

        var l = second.length,
            i = first.length,
            j = 0;

        if (typeof l === "number") {
            for (; j < l; j++) {
                first[i++] = second[j];
            }

        } else {
            while (second[j] !== undefined) {
                first[i++] = second[j++];
            }
        }

        first.length = i;

        return first;
    };
    jQuery.noConflict = function (deep) {
        /// <summary>
        ///     Relinquish jQuery's control of the $ variable.
        /// </summary>
        /// <param name="deep" type="Boolean">
        ///     A Boolean indicating whether to remove all jQuery variables from the global scope (including jQuery itself).
        /// </param>
        /// <returns type="Object" />

        if (window.$ === jQuery) {
            window.$ = _$;
        }

        if (deep && window.jQuery === jQuery) {
            window.jQuery = _jQuery;
        }

        return jQuery;
    };
    jQuery.noData = {
        "embed": true,
        "object": 'clsid:D27CDB6E-AE6D-11cf-96B8-444553540000',
        "applet": true
    };
    jQuery.nodeName = function (elem, name) {

        return elem.nodeName && elem.nodeName.toUpperCase() === name.toUpperCase();
    };
    jQuery.noop = function () {
        /// <summary>
        ///     An empty function.
        /// </summary>
        /// <returns type="Function" />
    };
    jQuery.now = function () {
        /// <summary>
        ///     Return a number representing the current time.
        /// </summary>
        /// <returns type="Number" />

        return (new Date()).getTime();
    };
    jQuery.offset = {};
    jQuery.param = function (a, traditional) {
        /// <summary>
        ///     Create a serialized representation of an array or object, suitable for use in a URL query string or Ajax request.
        ///     &#10;1 - jQuery.param(obj) 
        ///     &#10;2 - jQuery.param(obj, traditional)
        /// </summary>
        /// <param name="a" type="Object">
        ///     An array or object to serialize.
        /// </param>
        /// <param name="traditional" type="Boolean">
        ///     A Boolean indicating whether to perform a traditional "shallow" serialization.
        /// </param>
        /// <returns type="String" />

        var prefix,
            s = [],
            add = function (key, value) {
                // If value is a function, invoke it and return its value
                value = jQuery.isFunction(value) ? value() : (value == null ? "" : value);
                s[s.length] = encodeURIComponent(key) + "=" + encodeURIComponent(value);
            };

        // Set traditional to true for jQuery <= 1.3.2 behavior.
        if (traditional === undefined) {
            traditional = jQuery.ajaxSettings && jQuery.ajaxSettings.traditional;
        }

        // If an array was passed in, assume that it is an array of form elements.
        if (jQuery.isArray(a) || (a.jquery && !jQuery.isPlainObject(a))) {
            // Serialize the form elements
            jQuery.each(a, function () {
                add(this.name, this.value);
            });

        } else {
            // If traditional, encode the "old" way (the way 1.3.2 or older
            // did it), otherwise encode params recursively.
            for (prefix in a) {
                buildParams(prefix, a[prefix], traditional, add);
            }
        }

        // Return the resulting serialization
        return s.join("&").replace(r20, "+");
    };
    jQuery.parseHTML = function (data, context, scripts) {

        var parsed;
        if (!data || typeof data !== "string") {
            return null;
        }
        if (typeof context === "boolean") {
            scripts = context;
            context = 0;
        }
        context = context || document;

        // Single tag
        if ((parsed = rsingleTag.exec(data))) {
            return [context.createElement(parsed[1])];
        }

        parsed = jQuery.buildFragment([data], context, scripts ? null : []);
        return jQuery.merge([],
            (parsed.cacheable ? jQuery.clone(parsed.fragment) : parsed.fragment).childNodes);
    };
    jQuery.parseJSON = function (data) {
        /// <summary>
        ///     Takes a well-formed JSON string and returns the resulting JavaScript object.
        /// </summary>
        /// <param name="data" type="String">
        ///     The JSON string to parse.
        /// </param>
        /// <returns type="Object" />

        if (!data || typeof data !== "string") {
            return null;
        }

        // Make sure leading/trailing whitespace is removed (IE can't handle it)
        data = jQuery.trim(data);

        // Attempt to parse using the native JSON parser first
        if (window.JSON && window.JSON.parse) {
            return window.JSON.parse(data);
        }

        // Make sure the incoming data is actual JSON
        // Logic borrowed from http://json.org/json2.js
        if (rvalidchars.test(data.replace(rvalidescape, "@")
            .replace(rvalidtokens, "]")
            .replace(rvalidbraces, ""))) {

            return (new Function("return " + data))();

        }
        jQuery.error("Invalid JSON: " + data);
    };
    jQuery.parseXML = function (data) {
        /// <summary>
        ///     Parses a string into an XML document.
        /// </summary>
        /// <param name="data" type="String">
        ///     a well-formed XML string to be parsed
        /// </param>
        /// <returns type="XMLDocument" />

        var xml, tmp;
        if (!data || typeof data !== "string") {
            return null;
        }
        try {
            if (window.DOMParser) { // Standard
                tmp = new DOMParser();
                xml = tmp.parseFromString(data, "text/xml");
            } else { // IE
                xml = new ActiveXObject("Microsoft.XMLDOM");
                xml.async = "false";
                xml.loadXML(data);
            }
        } catch (e) {
            xml = undefined;
        }
        if (!xml || !xml.documentElement || xml.getElementsByTagName("parsererror").length) {
            jQuery.error("Invalid XML: " + data);
        }
        return xml;
    };
    jQuery.post = function (url, data, callback, type) {
        /// <summary>
        ///     Load data from the server using a HTTP POST request.
        /// </summary>
        /// <param name="url" type="String">
        ///     A string containing the URL to which the request is sent.
        /// </param>
        /// <param name="data" type="String">
        ///     A map or string that is sent to the server with the request.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A callback function that is executed if the request succeeds.
        /// </param>
        /// <param name="type" type="String">
        ///     The type of data expected from the server. Default: Intelligent Guess (xml, json, script, text, html).
        /// </param>

        // shift arguments if data argument was omitted
        if (jQuery.isFunction(data)) {
            type = type || callback;
            callback = data;
            data = undefined;
        }

        return jQuery.ajax({
            type: method,
            url: url,
            data: data,
            success: callback,
            dataType: type
        });
    };
    jQuery.prop = function (elem, name, value) {

        var ret, hooks, notxml,
            nType = elem.nodeType;

        // don't get/set properties on text, comment and attribute nodes
        if (!elem || nType === 3 || nType === 8 || nType === 2) {
            return;
        }

        notxml = nType !== 1 || !jQuery.isXMLDoc(elem);

        if (notxml) {
            // Fix name and attach hooks
            name = jQuery.propFix[name] || name;
            hooks = jQuery.propHooks[name];
        }

        if (value !== undefined) {
            if (hooks && "set" in hooks && (ret = hooks.set(elem, value, name)) !== undefined) {
                return ret;

            } else {
                return (elem[name] = value);
            }

        } else {
            if (hooks && "get" in hooks && (ret = hooks.get(elem, name)) !== null) {
                return ret;

            } else {
                return elem[name];
            }
        }
    };
    jQuery.propFix = {
        "tabindex": 'tabIndex',
        "readonly": 'readOnly',
        "for": 'htmlFor',
        "class": 'className',
        "maxlength": 'maxLength',
        "cellspacing": 'cellSpacing',
        "cellpadding": 'cellPadding',
        "rowspan": 'rowSpan',
        "colspan": 'colSpan',
        "usemap": 'useMap',
        "frameborder": 'frameBorder',
        "contenteditable": 'contentEditable'
    };
    jQuery.propHooks = { "tabIndex": {} };
    jQuery.proxy = function (fn, context) {
        /// <summary>
        ///     Takes a function and returns a new one that will always have a particular context.
        ///     &#10;1 - jQuery.proxy(function, context) 
        ///     &#10;2 - jQuery.proxy(context, name)
        /// </summary>
        /// <param name="fn" type="Function">
        ///     The function whose context will be changed.
        /// </param>
        /// <param name="context" type="Object">
        ///     The object to which the context (this) of the function should be set.
        /// </param>
        /// <returns type="Function" />

        var tmp, args, proxy;

        if (typeof context === "string") {
            tmp = fn[context];
            context = fn;
            fn = tmp;
        }

        // Quick check to determine if target is callable, in the spec
        // this throws a TypeError, but we will just return undefined.
        if (!jQuery.isFunction(fn)) {
            return undefined;
        }

        // Simulated bind
        args = core_slice.call(arguments, 2);
        proxy = function () {
            return fn.apply(context, args.concat(core_slice.call(arguments)));
        };

        // Set the guid of unique handler to the same of original handler, so it can be removed
        proxy.guid = fn.guid = fn.guid || proxy.guid || jQuery.guid++;

        return proxy;
    };
    jQuery.queue = function (elem, type, data) {
        /// <summary>
        ///     1: Show the queue of functions to be executed on the matched element.
        ///     &#10;    1.1 - jQuery.queue(element, queueName)
        ///     &#10;2: Manipulate the queue of functions to be executed on the matched element.
        ///     &#10;    2.1 - jQuery.queue(element, queueName, newQueue) 
        ///     &#10;    2.2 - jQuery.queue(element, queueName, callback())
        /// </summary>
        /// <param name="elem" domElement="true">
        ///     A DOM element where the array of queued functions is attached.
        /// </param>
        /// <param name="type" type="String">
        ///     A string containing the name of the queue. Defaults to fx, the standard effects queue.
        /// </param>
        /// <param name="data" type="Array">
        ///     An array of functions to replace the current queue contents.
        /// </param>
        /// <returns type="jQuery" />

        var queue;

        if (elem) {
            type = (type || "fx") + "queue";
            queue = jQuery._data(elem, type);

            // Speed up dequeue by getting out quickly if this is just a lookup
            if (data) {
                if (!queue || jQuery.isArray(data)) {
                    queue = jQuery._data(elem, type, jQuery.makeArray(data));
                } else {
                    queue.push(data);
                }
            }
            return queue || [];
        }
    };
    jQuery.ready = function (wait) {


        // Abort if there are pending holds or we're already ready
        if (wait === true ? --jQuery.readyWait : jQuery.isReady) {
            return;
        }

        // Make sure body exists, at least, in case IE gets a little overzealous (ticket #5443).
        if (!document.body) {
            return setTimeout(jQuery.ready, 1);
        }

        // Remember that the DOM is ready
        jQuery.isReady = true;

        // If a normal DOM Ready event fired, decrement, and wait if need be
        if (wait !== true && --jQuery.readyWait > 0) {
            return;
        }

        // If there are functions bound, to execute
        readyList.resolveWith(document, [jQuery]);

        // Trigger any bound ready events
        if (jQuery.fn.trigger) {
            jQuery(document).trigger("ready").off("ready");
        }
    };
    jQuery.readyWait = 0;
    jQuery.removeAttr = function (elem, value) {

        var propName, attrNames, name, isBool,
            i = 0;

        if (value && elem.nodeType === 1) {

            attrNames = value.split(core_rspace);

            for (; i < attrNames.length; i++) {
                name = attrNames[i];

                if (name) {
                    propName = jQuery.propFix[name] || name;
                    isBool = rboolean.test(name);

                    // See #9699 for explanation of this approach (setting first, then removal)
                    // Do not do this for boolean attributes (see #10870)
                    if (!isBool) {
                        jQuery.attr(elem, name, "");
                    }
                    elem.removeAttribute(getSetAttribute ? name : propName);

                    // Set corresponding property to false for boolean attributes
                    if (isBool && propName in elem) {
                        elem[propName] = false;
                    }
                }
            }
        }
    };
    jQuery.removeData = function (elem, name, pvt /* Internal Use Only */) {
        /// <summary>
        ///     Remove a previously-stored piece of data.
        /// </summary>
        /// <param name="elem" domElement="true">
        ///     A DOM element from which to remove data.
        /// </param>
        /// <param name="name" type="String">
        ///     A string naming the piece of data to remove.
        /// </param>
        /// <returns type="jQuery" />

        if (!jQuery.acceptData(elem)) {
            return;
        }

        var thisCache, i, l,

            isNode = elem.nodeType,

            // See jQuery.data for more information
            cache = isNode ? jQuery.cache : elem,
            id = isNode ? elem[jQuery.expando] : jQuery.expando;

        // If there is already no cache entry for this object, there is no
        // purpose in continuing
        if (!cache[id]) {
            return;
        }

        if (name) {

            thisCache = pvt ? cache[id] : cache[id].data;

            if (thisCache) {

                // Support array or space separated string names for data keys
                if (!jQuery.isArray(name)) {

                    // try the string as a key before any manipulation
                    if (name in thisCache) {
                        name = [name];
                    } else {

                        // split the camel cased version by spaces unless a key with the spaces exists
                        name = jQuery.camelCase(name);
                        if (name in thisCache) {
                            name = [name];
                        } else {
                            name = name.split(" ");
                        }
                    }
                }

                for (i = 0, l = name.length; i < l; i++) {
                    delete thisCache[name[i]];
                }

                // If there is no data left in the cache, we want to continue
                // and let the cache object itself get destroyed
                if (!(pvt ? isEmptyDataObject : jQuery.isEmptyObject)(thisCache)) {
                    return;
                }
            }
        }

        // See jQuery.data for more information
        if (!pvt) {
            delete cache[id].data;

            // Don't destroy the parent cache unless the internal data object
            // had been the only thing left in it
            if (!isEmptyDataObject(cache[id])) {
                return;
            }
        }

        // Destroy the cache
        if (isNode) {
            jQuery.cleanData([elem], true);

            // Use delete when supported for expandos or `cache` is not a window per isWindow (#10080)
        } else if (jQuery.support.deleteExpando || cache != cache.window) {
            delete cache[id];

            // When all else fails, null
        } else {
            cache[id] = null;
        }
    };
    jQuery.removeEvent = function (elem, type, handle) {

        if (elem.removeEventListener) {
            elem.removeEventListener(type, handle, false);
        }
    };
    jQuery.sibling = function (n, elem) {

        var r = [];

        for (; n; n = n.nextSibling) {
            if (n.nodeType === 1 && n !== elem) {
                r.push(n);
            }
        }

        return r;
    };
    jQuery.speed = function (speed, easing, fn) {

        var opt = speed && typeof speed === "object" ? jQuery.extend({}, speed) : {
            complete: fn || !fn && easing ||
                jQuery.isFunction(speed) && speed,
            duration: speed,
            easing: fn && easing || easing && !jQuery.isFunction(easing) && easing
        };

        opt.duration = jQuery.fx.off ? 0 : typeof opt.duration === "number" ? opt.duration :
            opt.duration in jQuery.fx.speeds ? jQuery.fx.speeds[opt.duration] : jQuery.fx.speeds._default;

        // normalize opt.queue - true/undefined/null -> "fx"
        if (opt.queue == null || opt.queue === true) {
            opt.queue = "fx";
        }

        // Queueing
        opt.old = opt.complete;

        opt.complete = function () {
            if (jQuery.isFunction(opt.old)) {
                opt.old.call(this);
            }

            if (opt.queue) {
                jQuery.dequeue(this, opt.queue);
            }
        };

        return opt;
    };
    jQuery.style = function (elem, name, value, extra) {

        // Don't set styles on text and comment nodes
        if (!elem || elem.nodeType === 3 || elem.nodeType === 8 || !elem.style) {
            return;
        }

        // Make sure that we're working with the right name
        var ret, type, hooks,
            origName = jQuery.camelCase(name),
            style = elem.style;

        name = jQuery.cssProps[origName] || (jQuery.cssProps[origName] = vendorPropName(style, origName));

        // gets hook for the prefixed version
        // followed by the unprefixed version
        hooks = jQuery.cssHooks[name] || jQuery.cssHooks[origName];

        // Check if we're setting a value
        if (value !== undefined) {
            type = typeof value;

            // convert relative number strings (+= or -=) to relative numbers. #7345
            if (type === "string" && (ret = rrelNum.exec(value))) {
                value = (ret[1] + 1) * ret[2] + parseFloat(jQuery.css(elem, name));
                // Fixes bug #9237
                type = "number";
            }

            // Make sure that NaN and null values aren't set. See: #7116
            if (value == null || type === "number" && isNaN(value)) {
                return;
            }

            // If a number was passed in, add 'px' to the (except for certain CSS properties)
            if (type === "number" && !jQuery.cssNumber[origName]) {
                value += "px";
            }

            // If a hook was provided, use that value, otherwise just set the specified value
            if (!hooks || !("set" in hooks) || (value = hooks.set(elem, value, extra)) !== undefined) {
                // Wrapped to prevent IE from throwing errors when 'invalid' values are provided
                // Fixes bug #5509
                try {
                    style[name] = value;
                } catch (e) { }
            }

        } else {
            // If a hook was provided get the non-computed value from there
            if (hooks && "get" in hooks && (ret = hooks.get(elem, false, extra)) !== undefined) {
                return ret;
            }

            // Otherwise just get the value from the style object
            return style[name];
        }
    };
    jQuery.sub = function () {
        /// <summary>
        ///     Creates a new copy of jQuery whose properties and methods can be modified without affecting the original jQuery object.
        /// </summary>
        /// <returns type="jQuery" />

        function jQuerySub(selector, context) {
            return new jQuerySub.fn.init(selector, context);
        }
        jQuery.extend(true, jQuerySub, this);
        jQuerySub.superclass = this;
        jQuerySub.fn = jQuerySub.prototype = this();
        jQuerySub.fn.constructor = jQuerySub;
        jQuerySub.sub = this.sub;
        jQuerySub.fn.init = function init(selector, context) {
            if (context && context instanceof jQuery && !(context instanceof jQuerySub)) {
                context = jQuerySub(context);
            }

            return jQuery.fn.init.call(this, selector, context, rootjQuerySub);
        };
        jQuerySub.fn.init.prototype = jQuerySub.fn;
        var rootjQuerySub = jQuerySub(document);
        return jQuerySub;
    };
    jQuery.support = {
        "leadingWhitespace": true,
        "tbody": true,
        "htmlSerialize": true,
        "style": true,
        "hrefNormalized": true,
        "opacity": true,
        "cssFloat": true,
        "checkOn": true,
        "optSelected": true,
        "getSetAttribute": true,
        "enctype": true,
        "html5Clone": true,
        "boxModel": true,
        "submitBubbles": true,
        "changeBubbles": true,
        "focusinBubbles": false,
        "deleteExpando": true,
        "noCloneEvent": true,
        "inlineBlockNeedsLayout": false,
        "shrinkWrapBlocks": false,
        "reliableMarginRight": true,
        "boxSizingReliable": true,
        "pixelPosition": false,
        "noCloneChecked": true,
        "optDisabled": true,
        "radioValue": true,
        "checkClone": true,
        "appendChecked": true,
        "ajax": true,
        "cors": true,
        "reliableHiddenOffsets": true,
        "boxSizing": true,
        "doesNotIncludeMarginInBodyOffset": true
    };
    jQuery.swap = function (elem, options, callback) {

        var ret, name,
            old = {};

        // Remember the old values, and insert the new ones
        for (name in options) {
            old[name] = elem.style[name];
            elem.style[name] = options[name];
        }

        ret = callback.call(elem);

        // Revert the old values
        for (name in options) {
            elem.style[name] = old[name];
        }

        return ret;
    };
    jQuery.text = function (elem) {

        var node,
            ret = "",
            i = 0,
            nodeType = elem.nodeType;

        if (nodeType) {
            if (nodeType === 1 || nodeType === 9 || nodeType === 11) {
                // Use textContent for elements
                // innerText usage removed for consistency of new lines (see #11153)
                if (typeof elem.textContent === "string") {
                    return elem.textContent;
                } else {
                    // Traverse its children
                    for (elem = elem.firstChild; elem; elem = elem.nextSibling) {
                        ret += getText(elem);
                    }
                }
            } else if (nodeType === 3 || nodeType === 4) {
                return elem.nodeValue;
            }
            // Do not include comment or processing instruction nodes
        } else {

            // If no nodeType, this is expected to be an array
            for (; (node = elem[i]) ; i++) {
                // Do not traverse comment nodes
                ret += getText(node);
            }
        }
        return ret;
    };
    jQuery.trim = function (text) {
        /// <summary>
        ///     Remove the whitespace from the beginning and end of a string.
        /// </summary>
        /// <param name="text" type="String">
        ///     The string to trim.
        /// </param>
        /// <returns type="String" />

        return text == null ?
            "" :
            core_trim.call(text);
    };
    jQuery.type = function (obj) {
        /// <summary>
        ///     Determine the internal JavaScript [[Class]] of an object.
        /// </summary>
        /// <param name="obj" type="Object">
        ///     Object to get the internal JavaScript [[Class]] of.
        /// </param>
        /// <returns type="String" />

        return obj == null ?
            String(obj) :
            class2type[core_toString.call(obj)] || "object";
    };
    jQuery.uaMatch = function (ua) {

        ua = ua.toLowerCase();

        var match = /(chrome)[ \/]([\w.]+)/.exec(ua) ||
            /(webkit)[ \/]([\w.]+)/.exec(ua) ||
            /(opera)(?:.*version|)[ \/]([\w.]+)/.exec(ua) ||
            /(msie) ([\w.]+)/.exec(ua) ||
            ua.indexOf("compatible") < 0 && /(mozilla)(?:.*? rv:([\w.]+)|)/.exec(ua) ||
            [];

        return {
            browser: match[1] || "",
            version: match[2] || "0"
        };
    };
    jQuery.unique = function (results) {
        /// <summary>
        ///     Sorts an array of DOM elements, in place, with the duplicates removed. Note that this only works on arrays of DOM elements, not strings or numbers.
        /// </summary>
        /// <param name="results" type="Array">
        ///     The Array of DOM elements.
        /// </param>
        /// <returns type="Array" />

        var elem,
            i = 1;

        hasDuplicate = baseHasDuplicate;
        results.sort(sortOrder);

        if (hasDuplicate) {
            for (; (elem = results[i]) ; i++) {
                if (elem === results[i - 1]) {
                    results.splice(i--, 1);
                }
            }
        }

        return results;
    };
    jQuery.uuid = 0;
    jQuery.valHooks = {
        "option": {},
        "select": {},
        "radio": {},
        "checkbox": {}
    };
    jQuery.when = function (subordinate /* , ..., subordinateN */) {
        /// <summary>
        ///     Provides a way to execute callback functions based on one or more objects, usually Deferred objects that represent asynchronous events.
        /// </summary>
        /// <param name="subordinate/*" type="Deferred">
        ///     One or more Deferred objects, or plain JavaScript objects.
        /// </param>
        /// <returns type="Promise" />

        var i = 0,
            resolveValues = core_slice.call(arguments),
            length = resolveValues.length,

            // the count of uncompleted subordinates
            remaining = length !== 1 || (subordinate && jQuery.isFunction(subordinate.promise)) ? length : 0,

            // the master Deferred. If resolveValues consist of only a single Deferred, just use that.
            deferred = remaining === 1 ? subordinate : jQuery.Deferred(),

            // Update function for both resolve and progress values
            updateFunc = function (i, contexts, values) {
                return function (value) {
                    contexts[i] = this;
                    values[i] = arguments.length > 1 ? core_slice.call(arguments) : value;
                    if (values === progressValues) {
                        deferred.notifyWith(contexts, values);
                    } else if (!(--remaining)) {
                        deferred.resolveWith(contexts, values);
                    }
                };
            },

            progressValues, progressContexts, resolveContexts;

        // add listeners to Deferred subordinates; treat others as resolved
        if (length > 1) {
            progressValues = new Array(length);
            progressContexts = new Array(length);
            resolveContexts = new Array(length);
            for (; i < length; i++) {
                if (resolveValues[i] && jQuery.isFunction(resolveValues[i].promise)) {
                    resolveValues[i].promise()
                        .done(updateFunc(i, resolveContexts, resolveValues))
                        .fail(deferred.reject)
                        .progress(updateFunc(i, progressContexts, progressValues));
                } else {
                    --remaining;
                }
            }
        }

        // if we're not waiting on anything, resolve the master
        if (!remaining) {
            deferred.resolveWith(resolveContexts, resolveValues);
        }

        return deferred.promise();
    };
    jQuery.Event.prototype.isDefaultPrevented = function returnFalse() {
        /// <summary>
        ///     Returns whether event.preventDefault() was ever called on this event object.
        /// </summary>
        /// <returns type="Boolean" />

        return false;
    };
    jQuery.Event.prototype.isImmediatePropagationStopped = function returnFalse() {
        /// <summary>
        ///     Returns whether event.stopImmediatePropagation() was ever called on this event object.
        /// </summary>
        /// <returns type="Boolean" />

        return false;
    };
    jQuery.Event.prototype.isPropagationStopped = function returnFalse() {
        /// <summary>
        ///     Returns whether event.stopPropagation() was ever called on this event object.
        /// </summary>
        /// <returns type="Boolean" />

        return false;
    };
    jQuery.Event.prototype.preventDefault = function () {
        /// <summary>
        ///     If this method is called, the default action of the event will not be triggered.
        /// </summary>
        /// <returns type="undefined" />

        this.isDefaultPrevented = returnTrue;

        var e = this.originalEvent;
        if (!e) {
            return;
        }

        // if preventDefault exists run it on the original event
        if (e.preventDefault) {
            e.preventDefault();

            // otherwise set the returnValue property of the original event to false (IE)
        } else {
            e.returnValue = false;
        }
    };
    jQuery.Event.prototype.stopImmediatePropagation = function () {
        /// <summary>
        ///     Keeps the rest of the handlers from being executed and prevents the event from bubbling up the DOM tree.
        /// </summary>

        this.isImmediatePropagationStopped = returnTrue;
        this.stopPropagation();
    };
    jQuery.Event.prototype.stopPropagation = function () {
        /// <summary>
        ///     Prevents the event from bubbling up the DOM tree, preventing any parent handlers from being notified of the event.
        /// </summary>

        this.isPropagationStopped = returnTrue;

        var e = this.originalEvent;
        if (!e) {
            return;
        }
        // if stopPropagation exists run it on the original event
        if (e.stopPropagation) {
            e.stopPropagation();
        }
        // otherwise set the cancelBubble property of the original event to true (IE)
        e.cancelBubble = true;
    };
    jQuery.prototype.add = function (selector, context) {
        /// <summary>
        ///     Add elements to the set of matched elements.
        ///     &#10;1 - add(selector) 
        ///     &#10;2 - add(elements) 
        ///     &#10;3 - add(html) 
        ///     &#10;4 - add(jQuery object) 
        ///     &#10;5 - add(selector, context)
        /// </summary>
        /// <param name="selector" type="String">
        ///     A string representing a selector expression to find additional elements to add to the set of matched elements.
        /// </param>
        /// <param name="context" domElement="true">
        ///     The point in the document at which the selector should begin matching; similar to the context argument of the $(selector, context) method.
        /// </param>
        /// <returns type="jQuery" />

        var set = typeof selector === "string" ?
                jQuery(selector, context) :
                jQuery.makeArray(selector && selector.nodeType ? [selector] : selector),
            all = jQuery.merge(this.get(), set);

        return this.pushStack(isDisconnected(set[0]) || isDisconnected(all[0]) ?
            all :
            jQuery.unique(all));
    };
    jQuery.prototype.addBack = function (selector) {

        return this.add(selector == null ?
            this.prevObject : this.prevObject.filter(selector)
        );
    };
    jQuery.prototype.addClass = function (value) {
        /// <summary>
        ///     Adds the specified class(es) to each of the set of matched elements.
        ///     &#10;1 - addClass(className) 
        ///     &#10;2 - addClass(function(index, currentClass))
        /// </summary>
        /// <param name="value" type="String">
        ///     One or more class names to be added to the class attribute of each matched element.
        /// </param>
        /// <returns type="jQuery" />

        var classNames, i, l, elem,
            setClass, c, cl;

        if (jQuery.isFunction(value)) {
            return this.each(function (j) {
                jQuery(this).addClass(value.call(this, j, this.className));
            });
        }

        if (value && typeof value === "string") {
            classNames = value.split(core_rspace);

            for (i = 0, l = this.length; i < l; i++) {
                elem = this[i];

                if (elem.nodeType === 1) {
                    if (!elem.className && classNames.length === 1) {
                        elem.className = value;

                    } else {
                        setClass = " " + elem.className + " ";

                        for (c = 0, cl = classNames.length; c < cl; c++) {
                            if (!~setClass.indexOf(" " + classNames[c] + " ")) {
                                setClass += classNames[c] + " ";
                            }
                        }
                        elem.className = jQuery.trim(setClass);
                    }
                }
            }
        }

        return this;
    };
    jQuery.prototype.after = function () {
        /// <summary>
        ///     Insert content, specified by the parameter, after each element in the set of matched elements.
        ///     &#10;1 - after(content, content) 
        ///     &#10;2 - after(function(index))
        /// </summary>
        /// <param name="" type="jQuery">
        ///     HTML string, DOM element, or jQuery object to insert after each element in the set of matched elements.
        /// </param>
        /// <param name="" type="jQuery">
        ///     One or more additional DOM elements, arrays of elements, HTML strings, or jQuery objects to insert after each element in the set of matched elements.
        /// </param>
        /// <returns type="jQuery" />

        if (!isDisconnected(this[0])) {
            return this.domManip(arguments, false, function (elem) {
                this.parentNode.insertBefore(elem, this.nextSibling);
            });
        }

        if (arguments.length) {
            var set = jQuery.clean(arguments);
            return this.pushStack(jQuery.merge(this, set), "after", this.selector);
        }
    };
    jQuery.prototype.ajaxComplete = function (f) {
        /// <summary>
        ///     Register a handler to be called when Ajax requests complete. This is an Ajax Event.
        /// </summary>
        /// <param name="f" type="Function">
        ///     The function to be invoked.
        /// </param>
        /// <returns type="jQuery" />

        return this.on(o, f);
    };
    jQuery.prototype.ajaxError = function (f) {
        /// <summary>
        ///     Register a handler to be called when Ajax requests complete with an error. This is an Ajax Event.
        /// </summary>
        /// <param name="f" type="Function">
        ///     The function to be invoked.
        /// </param>
        /// <returns type="jQuery" />

        return this.on(o, f);
    };
    jQuery.prototype.ajaxSend = function (f) {
        /// <summary>
        ///     Attach a function to be executed before an Ajax request is sent. This is an Ajax Event.
        /// </summary>
        /// <param name="f" type="Function">
        ///     The function to be invoked.
        /// </param>
        /// <returns type="jQuery" />

        return this.on(o, f);
    };
    jQuery.prototype.ajaxStart = function (f) {
        /// <summary>
        ///     Register a handler to be called when the first Ajax request begins. This is an Ajax Event.
        /// </summary>
        /// <param name="f" type="Function">
        ///     The function to be invoked.
        /// </param>
        /// <returns type="jQuery" />

        return this.on(o, f);
    };
    jQuery.prototype.ajaxStop = function (f) {
        /// <summary>
        ///     Register a handler to be called when all Ajax requests have completed. This is an Ajax Event.
        /// </summary>
        /// <param name="f" type="Function">
        ///     The function to be invoked.
        /// </param>
        /// <returns type="jQuery" />

        return this.on(o, f);
    };
    jQuery.prototype.ajaxSuccess = function (f) {
        /// <summary>
        ///     Attach a function to be executed whenever an Ajax request completes successfully. This is an Ajax Event.
        /// </summary>
        /// <param name="f" type="Function">
        ///     The function to be invoked.
        /// </param>
        /// <returns type="jQuery" />

        return this.on(o, f);
    };
    jQuery.prototype.andSelf = function (selector) {
        /// <summary>
        ///     Add the previous set of elements on the stack to the current set.
        /// </summary>
        /// <returns type="jQuery" />

        return this.add(selector == null ?
            this.prevObject : this.prevObject.filter(selector)
        );
    };
    jQuery.prototype.animate = function (prop, speed, easing, callback) {
        /// <summary>
        ///     Perform a custom animation of a set of CSS properties.
        ///     &#10;1 - animate(properties, duration, easing, complete) 
        ///     &#10;2 - animate(properties, options)
        /// </summary>
        /// <param name="prop" type="Object">
        ///     A map of CSS properties that the animation will move toward.
        /// </param>
        /// <param name="speed" type="Number">
        ///     A string or number determining how long the animation will run.
        /// </param>
        /// <param name="easing" type="String">
        ///     A string indicating which easing function to use for the transition.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A function to call once the animation is complete.
        /// </param>
        /// <returns type="jQuery" />

        var empty = jQuery.isEmptyObject(prop),
            optall = jQuery.speed(speed, easing, callback),
            doAnimation = function () {
                // Operate on a copy of prop so per-property easing won't be lost
                var anim = Animation(this, jQuery.extend({}, prop), optall);

                // Empty animations resolve immediately
                if (empty) {
                    anim.stop(true);
                }
            };

        return empty || optall.queue === false ?
            this.each(doAnimation) :
            this.queue(optall.queue, doAnimation);
    };
    jQuery.prototype.append = function () {
        /// <summary>
        ///     Insert content, specified by the parameter, to the end of each element in the set of matched elements.
        ///     &#10;1 - append(content, content) 
        ///     &#10;2 - append(function(index, html))
        /// </summary>
        /// <param name="" type="jQuery">
        ///     DOM element, HTML string, or jQuery object to insert at the end of each element in the set of matched elements.
        /// </param>
        /// <param name="" type="jQuery">
        ///     One or more additional DOM elements, arrays of elements, HTML strings, or jQuery objects to insert at the end of each element in the set of matched elements.
        /// </param>
        /// <returns type="jQuery" />

        return this.domManip(arguments, true, function (elem) {
            if (this.nodeType === 1 || this.nodeType === 11) {
                this.appendChild(elem);
            }
        });
    };
    jQuery.prototype.appendTo = function (selector) {
        /// <summary>
        ///     Insert every element in the set of matched elements to the end of the target.
        /// </summary>
        /// <param name="selector" type="jQuery">
        ///     A selector, element, HTML string, or jQuery object; the matched set of elements will be inserted at the end of the element(s) specified by this parameter.
        /// </param>
        /// <returns type="jQuery" />

        var elems,
            i = 0,
            ret = [],
            insert = jQuery(selector),
            l = insert.length,
            parent = this.length === 1 && this[0].parentNode;

        if ((parent == null || parent && parent.nodeType === 11 && parent.childNodes.length === 1) && l === 1) {
            insert[original](this[0]);
            return this;
        } else {
            for (; i < l; i++) {
                elems = (i > 0 ? this.clone(true) : this).get();
                jQuery(insert[i])[original](elems);
                ret = ret.concat(elems);
            }

            return this.pushStack(ret, name, insert.selector);
        }
    };
    jQuery.prototype.attr = function (name, value) {
        /// <summary>
        ///     1: Get the value of an attribute for the first element in the set of matched elements.
        ///     &#10;    1.1 - attr(attributeName)
        ///     &#10;2: Set one or more attributes for the set of matched elements.
        ///     &#10;    2.1 - attr(attributeName, value) 
        ///     &#10;    2.2 - attr(map) 
        ///     &#10;    2.3 - attr(attributeName, function(index, attr))
        /// </summary>
        /// <param name="name" type="String">
        ///     The name of the attribute to set.
        /// </param>
        /// <param name="value" type="Number">
        ///     A value to set for the attribute.
        /// </param>
        /// <returns type="jQuery" />

        return jQuery.access(this, jQuery.attr, name, value, arguments.length > 1);
    };
    jQuery.prototype.before = function () {
        /// <summary>
        ///     Insert content, specified by the parameter, before each element in the set of matched elements.
        ///     &#10;1 - before(content, content) 
        ///     &#10;2 - before(function)
        /// </summary>
        /// <param name="" type="jQuery">
        ///     HTML string, DOM element, or jQuery object to insert before each element in the set of matched elements.
        /// </param>
        /// <param name="" type="jQuery">
        ///     One or more additional DOM elements, arrays of elements, HTML strings, or jQuery objects to insert before each element in the set of matched elements.
        /// </param>
        /// <returns type="jQuery" />

        if (!isDisconnected(this[0])) {
            return this.domManip(arguments, false, function (elem) {
                this.parentNode.insertBefore(elem, this);
            });
        }

        if (arguments.length) {
            var set = jQuery.clean(arguments);
            return this.pushStack(jQuery.merge(set, this), "before", this.selector);
        }
    };
    jQuery.prototype.bind = function (types, data, fn) {
        /// <summary>
        ///     Attach a handler to an event for the elements.
        ///     &#10;1 - bind(eventType, eventData, handler(eventObject)) 
        ///     &#10;2 - bind(eventType, eventData, preventBubble) 
        ///     &#10;3 - bind(events)
        /// </summary>
        /// <param name="types" type="String">
        ///     A string containing one or more DOM event types, such as "click" or "submit," or custom event names.
        /// </param>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        return this.on(types, null, data, fn);
    };
    jQuery.prototype.blur = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "blur" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - blur(handler(eventObject)) 
        ///     &#10;2 - blur(eventData, handler(eventObject)) 
        ///     &#10;3 - blur()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.change = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "change" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - change(handler(eventObject)) 
        ///     &#10;2 - change(eventData, handler(eventObject)) 
        ///     &#10;3 - change()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.children = function (until, selector) {
        /// <summary>
        ///     Get the children of each element in the set of matched elements, optionally filtered by a selector.
        /// </summary>
        /// <param name="until" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.clearQueue = function (type) {
        /// <summary>
        ///     Remove from the queue all items that have not yet been run.
        /// </summary>
        /// <param name="type" type="String">
        ///     A string containing the name of the queue. Defaults to fx, the standard effects queue.
        /// </param>
        /// <returns type="jQuery" />

        return this.queue(type || "fx", []);
    };
    jQuery.prototype.click = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "click" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - click(handler(eventObject)) 
        ///     &#10;2 - click(eventData, handler(eventObject)) 
        ///     &#10;3 - click()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.clone = function (dataAndEvents, deepDataAndEvents) {
        /// <summary>
        ///     Create a deep copy of the set of matched elements.
        ///     &#10;1 - clone(withDataAndEvents) 
        ///     &#10;2 - clone(withDataAndEvents, deepWithDataAndEvents)
        /// </summary>
        /// <param name="dataAndEvents" type="Boolean">
        ///     A Boolean indicating whether event handlers and data should be copied along with the elements. The default value is false. *In jQuery 1.5.0 the default value was incorrectly true; it was changed back to false in 1.5.1 and up.
        /// </param>
        /// <param name="deepDataAndEvents" type="Boolean">
        ///     A Boolean indicating whether event handlers and data for all children of the cloned element should be copied. By default its value matches the first argument's value (which defaults to false).
        /// </param>
        /// <returns type="jQuery" />

        dataAndEvents = dataAndEvents == null ? false : dataAndEvents;
        deepDataAndEvents = deepDataAndEvents == null ? dataAndEvents : deepDataAndEvents;

        return this.map(function () {
            return jQuery.clone(this, dataAndEvents, deepDataAndEvents);
        });
    };
    jQuery.prototype.closest = function (selectors, context) {
        /// <summary>
        ///     1: Get the first element that matches the selector, beginning at the current element and progressing up through the DOM tree.
        ///     &#10;    1.1 - closest(selector) 
        ///     &#10;    1.2 - closest(selector, context) 
        ///     &#10;    1.3 - closest(jQuery object) 
        ///     &#10;    1.4 - closest(element)
        ///     &#10;2: Gets an array of all the elements and selectors matched against the current element up through the DOM tree.
        ///     &#10;    2.1 - closest(selectors, context)
        /// </summary>
        /// <param name="selectors" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <param name="context" domElement="true">
        ///     A DOM element within which a matching element may be found. If no context is passed in then the context of the jQuery set will be used instead.
        /// </param>
        /// <returns type="jQuery" />

        var cur,
            i = 0,
            l = this.length,
            ret = [],
            pos = rneedsContext.test(selectors) || typeof selectors !== "string" ?
                jQuery(selectors, context || this.context) :
                0;

        for (; i < l; i++) {
            cur = this[i];

            while (cur && cur.ownerDocument && cur !== context && cur.nodeType !== 11) {
                if (pos ? pos.index(cur) > -1 : jQuery.find.matchesSelector(cur, selectors)) {
                    ret.push(cur);
                    break;
                }
                cur = cur.parentNode;
            }
        }

        ret = ret.length > 1 ? jQuery.unique(ret) : ret;

        return this.pushStack(ret, "closest", selectors);
    };
    jQuery.prototype.constructor = function (selector, context) {

        // The jQuery object is actually just the init constructor 'enhanced'
        return new jQuery.fn.init(selector, context, rootjQuery);
    };
    jQuery.prototype.contents = function (until, selector) {
        /// <summary>
        ///     Get the children of each element in the set of matched elements, including text and comment nodes.
        /// </summary>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.contextmenu = function (data, fn) {

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.css = function (name, value) {
        /// <summary>
        ///     1: Get the value of a style property for the first element in the set of matched elements.
        ///     &#10;    1.1 - css(propertyName)
        ///     &#10;2: Set one or more CSS properties for the  set of matched elements.
        ///     &#10;    2.1 - css(propertyName, value) 
        ///     &#10;    2.2 - css(propertyName, function(index, value)) 
        ///     &#10;    2.3 - css(map)
        /// </summary>
        /// <param name="name" type="String">
        ///     A CSS property name.
        /// </param>
        /// <param name="value" type="Number">
        ///     A value to set for the property.
        /// </param>
        /// <returns type="jQuery" />

        return jQuery.access(this, function (elem, name, value) {
            return value !== undefined ?
                jQuery.style(elem, name, value) :
                jQuery.css(elem, name);
        }, name, value, arguments.length > 1);
    };
    jQuery.prototype.data = function (key, value) {
        /// <summary>
        ///     1: Store arbitrary data associated with the matched elements.
        ///     &#10;    1.1 - data(key, value) 
        ///     &#10;    1.2 - data(obj)
        ///     &#10;2: Returns value at named data store for the first element in the jQuery collection, as set by data(name, value).
        ///     &#10;    2.1 - data(key) 
        ///     &#10;    2.2 - data()
        /// </summary>
        /// <param name="key" type="String">
        ///     A string naming the piece of data to set.
        /// </param>
        /// <param name="value" type="Object">
        ///     The new data value; it can be any Javascript type including Array or Object.
        /// </param>
        /// <returns type="jQuery" />

        var parts, part, attr, name, l,
            elem = this[0],
            i = 0,
            data = null;

        // Gets all values
        if (key === undefined) {
            if (this.length) {
                data = jQuery.data(elem);

                if (elem.nodeType === 1 && !jQuery._data(elem, "parsedAttrs")) {
                    attr = elem.attributes;
                    for (l = attr.length; i < l; i++) {
                        name = attr[i].name;

                        if (name.indexOf("data-") === 0) {
                            name = jQuery.camelCase(name.substring(5));

                            dataAttr(elem, name, data[name]);
                        }
                    }
                    jQuery._data(elem, "parsedAttrs", true);
                }
            }

            return data;
        }

        // Sets multiple values
        if (typeof key === "object") {
            return this.each(function () {
                jQuery.data(this, key);
            });
        }

        parts = key.split(".", 2);
        parts[1] = parts[1] ? "." + parts[1] : "";
        part = parts[1] + "!";

        return jQuery.access(this, function (value) {

            if (value === undefined) {
                data = this.triggerHandler("getData" + part, [parts[0]]);

                // Try to fetch any internally stored data first
                if (data === undefined && elem) {
                    data = jQuery.data(elem, key);
                    data = dataAttr(elem, key, data);
                }

                return data === undefined && parts[1] ?
                    this.data(parts[0]) :
                    data;
            }

            parts[1] = value;
            this.each(function () {
                var self = jQuery(this);

                self.triggerHandler("setData" + part, parts);
                jQuery.data(this, key, value);
                self.triggerHandler("changeData" + part, parts);
            });
        }, null, value, arguments.length > 1, null, false);
    };
    jQuery.prototype.dblclick = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "dblclick" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - dblclick(handler(eventObject)) 
        ///     &#10;2 - dblclick(eventData, handler(eventObject)) 
        ///     &#10;3 - dblclick()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.delay = function (time, type) {
        /// <summary>
        ///     Set a timer to delay execution of subsequent items in the queue.
        /// </summary>
        /// <param name="time" type="Number">
        ///     An integer indicating the number of milliseconds to delay execution of the next item in the queue.
        /// </param>
        /// <param name="type" type="String">
        ///     A string containing the name of the queue. Defaults to fx, the standard effects queue.
        /// </param>
        /// <returns type="jQuery" />

        time = jQuery.fx ? jQuery.fx.speeds[time] || time : time;
        type = type || "fx";

        return this.queue(type, function (next, hooks) {
            var timeout = setTimeout(next, time);
            hooks.stop = function () {
                clearTimeout(timeout);
            };
        });
    };
    jQuery.prototype.delegate = function (selector, types, data, fn) {
        /// <summary>
        ///     Attach a handler to one or more events for all elements that match the selector, now or in the future, based on a specific set of root elements.
        ///     &#10;1 - delegate(selector, eventType, handler(eventObject)) 
        ///     &#10;2 - delegate(selector, eventType, eventData, handler(eventObject)) 
        ///     &#10;3 - delegate(selector, events)
        /// </summary>
        /// <param name="selector" type="String">
        ///     A selector to filter the elements that trigger the event.
        /// </param>
        /// <param name="types" type="String">
        ///     A string containing one or more space-separated JavaScript event types, such as "click" or "keydown," or custom event names.
        /// </param>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute at the time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        return this.on(types, selector, data, fn);
    };
    jQuery.prototype.dequeue = function (type) {
        /// <summary>
        ///     Execute the next function on the queue for the matched elements.
        /// </summary>
        /// <param name="type" type="String">
        ///     A string containing the name of the queue. Defaults to fx, the standard effects queue.
        /// </param>
        /// <returns type="jQuery" />

        return this.each(function () {
            jQuery.dequeue(this, type);
        });
    };
    jQuery.prototype.detach = function (selector) {
        /// <summary>
        ///     Remove the set of matched elements from the DOM.
        /// </summary>
        /// <param name="selector" type="String">
        ///     A selector expression that filters the set of matched elements to be removed.
        /// </param>
        /// <returns type="jQuery" />

        return this.remove(selector, true);
    };
    jQuery.prototype.die = function (types, fn) {
        /// <summary>
        ///     1: Remove all event handlers previously attached using .live() from the elements.
        ///     &#10;    1.1 - die()
        ///     &#10;2: Remove an event handler previously attached using .live() from the elements.
        ///     &#10;    2.1 - die(eventType, handler) 
        ///     &#10;    2.2 - die(eventTypes)
        /// </summary>
        /// <param name="types" type="String">
        ///     A string containing a JavaScript event type, such as click or keydown.
        /// </param>
        /// <param name="fn" type="String">
        ///     The function that is no longer to be executed.
        /// </param>
        /// <returns type="jQuery" />

        jQuery(this.context).off(types, this.selector || "**", fn);
        return this;
    };
    jQuery.prototype.domManip = function (args, table, callback) {


        // Flatten any nested arrays
        args = [].concat.apply([], args);

        var results, first, fragment, iNoClone,
            i = 0,
            value = args[0],
            scripts = [],
            l = this.length;

        // We can't cloneNode fragments that contain checked, in WebKit
        if (!jQuery.support.checkClone && l > 1 && typeof value === "string" && rchecked.test(value)) {
            return this.each(function () {
                jQuery(this).domManip(args, table, callback);
            });
        }

        if (jQuery.isFunction(value)) {
            return this.each(function (i) {
                var self = jQuery(this);
                args[0] = value.call(this, i, table ? self.html() : undefined);
                self.domManip(args, table, callback);
            });
        }

        if (this[0]) {
            results = jQuery.buildFragment(args, this, scripts);
            fragment = results.fragment;
            first = fragment.firstChild;

            if (fragment.childNodes.length === 1) {
                fragment = first;
            }

            if (first) {
                table = table && jQuery.nodeName(first, "tr");

                // Use the original fragment for the last item instead of the first because it can end up
                // being emptied incorrectly in certain situations (#8070).
                // Fragments from the fragment cache must always be cloned and never used in place.
                for (iNoClone = results.cacheable || l - 1; i < l; i++) {
                    callback.call(
                        table && jQuery.nodeName(this[i], "table") ?
                            findOrAppend(this[i], "tbody") :
                            this[i],
                        i === iNoClone ?
                        fragment :
                            jQuery.clone(fragment, true, true)
                    );
                }
            }

            // Fix #11809: Avoid leaking memory
            fragment = first = null;

            if (scripts.length) {
                jQuery.each(scripts, function (i, elem) {
                    if (elem.src) {
                        if (jQuery.ajax) {
                            jQuery.ajax({
                                url: elem.src,
                                type: "GET",
                                dataType: "script",
                                async: false,
                                global: false,
                                "throws": true
                            });
                        } else {
                            jQuery.error("no ajax");
                        }
                    } else {
                        jQuery.globalEval((elem.text || elem.textContent || elem.innerHTML || "").replace(rcleanScript, ""));
                    }

                    if (elem.parentNode) {
                        elem.parentNode.removeChild(elem);
                    }
                });
            }
        }

        return this;
    };
    jQuery.prototype.each = function (callback, args) {
        /// <summary>
        ///     Iterate over a jQuery object, executing a function for each matched element.
        /// </summary>
        /// <param name="callback" type="Function">
        ///     A function to execute for each matched element.
        /// </param>
        /// <returns type="jQuery" />

        return jQuery.each(this, callback, args);
    };
    jQuery.prototype.empty = function () {
        /// <summary>
        ///     Remove all child nodes of the set of matched elements from the DOM.
        /// </summary>
        /// <returns type="jQuery" />

        var elem,
            i = 0;

        for (; (elem = this[i]) != null; i++) {
            // Remove element nodes and prevent memory leaks
            if (elem.nodeType === 1) {
                jQuery.cleanData(elem.getElementsByTagName("*"));
            }

            // Remove any remaining nodes
            while (elem.firstChild) {
                elem.removeChild(elem.firstChild);
            }
        }

        return this;
    };
    jQuery.prototype.end = function () {
        /// <summary>
        ///     End the most recent filtering operation in the current chain and return the set of matched elements to its previous state.
        /// </summary>
        /// <returns type="jQuery" />

        return this.prevObject || this.constructor(null);
    };
    jQuery.prototype.eq = function (i) {
        /// <summary>
        ///     Reduce the set of matched elements to the one at the specified index.
        ///     &#10;1 - eq(index) 
        ///     &#10;2 - eq(-index)
        /// </summary>
        /// <param name="i" type="Number">
        ///     An integer indicating the 0-based position of the element.
        /// </param>
        /// <returns type="jQuery" />

        i = +i;
        return i === -1 ?
            this.slice(i) :
            this.slice(i, i + 1);
    };
    jQuery.prototype.error = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "error" JavaScript event.
        ///     &#10;1 - error(handler(eventObject)) 
        ///     &#10;2 - error(eventData, handler(eventObject))
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.extend = function () {

        var options, name, src, copy, copyIsArray, clone,
            target = arguments[0] || {},
            i = 1,
            length = arguments.length,
            deep = false;

        // Handle a deep copy situation
        if (typeof target === "boolean") {
            deep = target;
            target = arguments[1] || {};
            // skip the boolean and the target
            i = 2;
        }

        // Handle case when target is a string or something (possible in deep copy)
        if (typeof target !== "object" && !jQuery.isFunction(target)) {
            target = {};
        }

        // extend jQuery itself if only one argument is passed
        if (length === i) {
            target = this;
            --i;
        }

        for (; i < length; i++) {
            // Only deal with non-null/undefined values
            if ((options = arguments[i]) != null) {
                // Extend the base object
                for (name in options) {
                    src = target[name];
                    copy = options[name];

                    // Prevent never-ending loop
                    if (target === copy) {
                        continue;
                    }

                    // Recurse if we're merging plain objects or arrays
                    if (deep && copy && (jQuery.isPlainObject(copy) || (copyIsArray = jQuery.isArray(copy)))) {
                        if (copyIsArray) {
                            copyIsArray = false;
                            clone = src && jQuery.isArray(src) ? src : [];

                        } else {
                            clone = src && jQuery.isPlainObject(src) ? src : {};
                        }

                        // Never move original objects, clone them
                        target[name] = jQuery.extend(deep, clone, copy);

                        // Don't bring in undefined values
                    } else if (copy !== undefined) {
                        target[name] = copy;
                    }
                }
            }
        }

        // Return the modified object
        return target;
    };
    jQuery.prototype.fadeIn = function (speed, easing, callback) {
        /// <summary>
        ///     Display the matched elements by fading them to opaque.
        ///     &#10;1 - fadeIn(duration, callback) 
        ///     &#10;2 - fadeIn(duration, easing, callback)
        /// </summary>
        /// <param name="speed" type="Number">
        ///     A string or number determining how long the animation will run.
        /// </param>
        /// <param name="easing" type="String">
        ///     A string indicating which easing function to use for the transition.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A function to call once the animation is complete.
        /// </param>
        /// <returns type="jQuery" />

        return this.animate(props, speed, easing, callback);
    };
    jQuery.prototype.fadeOut = function (speed, easing, callback) {
        /// <summary>
        ///     Hide the matched elements by fading them to transparent.
        ///     &#10;1 - fadeOut(duration, callback) 
        ///     &#10;2 - fadeOut(duration, easing, callback)
        /// </summary>
        /// <param name="speed" type="Number">
        ///     A string or number determining how long the animation will run.
        /// </param>
        /// <param name="easing" type="String">
        ///     A string indicating which easing function to use for the transition.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A function to call once the animation is complete.
        /// </param>
        /// <returns type="jQuery" />

        return this.animate(props, speed, easing, callback);
    };
    jQuery.prototype.fadeTo = function (speed, to, easing, callback) {
        /// <summary>
        ///     Adjust the opacity of the matched elements.
        ///     &#10;1 - fadeTo(duration, opacity, callback) 
        ///     &#10;2 - fadeTo(duration, opacity, easing, callback)
        /// </summary>
        /// <param name="speed" type="Number">
        ///     A string or number determining how long the animation will run.
        /// </param>
        /// <param name="to" type="Number">
        ///     A number between 0 and 1 denoting the target opacity.
        /// </param>
        /// <param name="easing" type="String">
        ///     A string indicating which easing function to use for the transition.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A function to call once the animation is complete.
        /// </param>
        /// <returns type="jQuery" />


        // show any hidden elements after setting opacity to 0
        return this.filter(isHidden).css("opacity", 0).show()

            // animate to the value specified
            .end().animate({ opacity: to }, speed, easing, callback);
    };
    jQuery.prototype.fadeToggle = function (speed, easing, callback) {
        /// <summary>
        ///     Display or hide the matched elements by animating their opacity.
        /// </summary>
        /// <param name="speed" type="Number">
        ///     A string or number determining how long the animation will run.
        /// </param>
        /// <param name="easing" type="String">
        ///     A string indicating which easing function to use for the transition.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A function to call once the animation is complete.
        /// </param>
        /// <returns type="jQuery" />

        return this.animate(props, speed, easing, callback);
    };
    jQuery.prototype.filter = function (selector) {
        /// <summary>
        ///     Reduce the set of matched elements to those that match the selector or pass the function's test.
        ///     &#10;1 - filter(selector) 
        ///     &#10;2 - filter(function(index)) 
        ///     &#10;3 - filter(element) 
        ///     &#10;4 - filter(jQuery object)
        /// </summary>
        /// <param name="selector" type="String">
        ///     A string containing a selector expression to match the current set of elements against.
        /// </param>
        /// <returns type="jQuery" />

        return this.pushStack(winnow(this, selector, true), "filter", selector);
    };
    jQuery.prototype.find = function (selector) {
        /// <summary>
        ///     Get the descendants of each element in the current set of matched elements, filtered by a selector, jQuery object, or element.
        ///     &#10;1 - find(selector) 
        ///     &#10;2 - find(jQuery object) 
        ///     &#10;3 - find(element)
        /// </summary>
        /// <param name="selector" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var i, l, length, n, r, ret,
            self = this;

        if (typeof selector !== "string") {
            return jQuery(selector).filter(function () {
                for (i = 0, l = self.length; i < l; i++) {
                    if (jQuery.contains(self[i], this)) {
                        return true;
                    }
                }
            });
        }

        ret = this.pushStack("", "find", selector);

        for (i = 0, l = this.length; i < l; i++) {
            length = ret.length;
            jQuery.find(selector, this[i], ret);

            if (i > 0) {
                // Make sure that the results are unique
                for (n = length; n < ret.length; n++) {
                    for (r = 0; r < length; r++) {
                        if (ret[r] === ret[n]) {
                            ret.splice(n--, 1);
                            break;
                        }
                    }
                }
            }
        }

        return ret;
    };
    jQuery.prototype.first = function () {
        /// <summary>
        ///     Reduce the set of matched elements to the first in the set.
        /// </summary>
        /// <returns type="jQuery" />

        return this.eq(0);
    };
    jQuery.prototype.focus = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "focus" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - focus(handler(eventObject)) 
        ///     &#10;2 - focus(eventData, handler(eventObject)) 
        ///     &#10;3 - focus()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.focusin = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "focusin" event.
        ///     &#10;1 - focusin(handler(eventObject)) 
        ///     &#10;2 - focusin(eventData, handler(eventObject))
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.focusout = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "focusout" JavaScript event.
        ///     &#10;1 - focusout(handler(eventObject)) 
        ///     &#10;2 - focusout(eventData, handler(eventObject))
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.get = function (num) {
        /// <summary>
        ///     Retrieve the DOM elements matched by the jQuery object.
        /// </summary>
        /// <param name="num" type="Number">
        ///     A zero-based integer indicating which element to retrieve.
        /// </param>
        /// <returns type="Array" />

        return num == null ?

            // Return a 'clean' array
            this.toArray() :

            // Return just the object
            (num < 0 ? this[this.length + num] : this[num]);
    };
    jQuery.prototype.has = function (target) {
        /// <summary>
        ///     Reduce the set of matched elements to those that have a descendant that matches the selector or DOM element.
        ///     &#10;1 - has(selector) 
        ///     &#10;2 - has(contained)
        /// </summary>
        /// <param name="target" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var i,
            targets = jQuery(target, this),
            len = targets.length;

        return this.filter(function () {
            for (i = 0; i < len; i++) {
                if (jQuery.contains(this, targets[i])) {
                    return true;
                }
            }
        });
    };
    jQuery.prototype.hasClass = function (selector) {
        /// <summary>
        ///     Determine whether any of the matched elements are assigned the given class.
        /// </summary>
        /// <param name="selector" type="String">
        ///     The class name to search for.
        /// </param>
        /// <returns type="Boolean" />

        var className = " " + selector + " ",
            i = 0,
            l = this.length;
        for (; i < l; i++) {
            if (this[i].nodeType === 1 && (" " + this[i].className + " ").replace(rclass, " ").indexOf(className) > -1) {
                return true;
            }
        }

        return false;
    };
    jQuery.prototype.height = function (margin, value) {
        /// <summary>
        ///     1: Get the current computed height for the first element in the set of matched elements.
        ///     &#10;    1.1 - height()
        ///     &#10;2: Set the CSS height of every matched element.
        ///     &#10;    2.1 - height(value) 
        ///     &#10;    2.2 - height(function(index, height))
        /// </summary>
        /// <param name="margin" type="Number">
        ///     An integer representing the number of pixels, or an integer with an optional unit of measure appended (as a string).
        /// </param>
        /// <returns type="jQuery" />

        var chainable = arguments.length && (defaultExtra || typeof margin !== "boolean"),
            extra = defaultExtra || (margin === true || value === true ? "margin" : "border");

        return jQuery.access(this, function (elem, type, value) {
            var doc;

            if (jQuery.isWindow(elem)) {
                // As of 5/8/2012 this will yield incorrect results for Mobile Safari, but there
                // isn't a whole lot we can do. See pull request at this URL for discussion:
                // https://github.com/jquery/jquery/pull/764
                return elem.document.documentElement["client" + name];
            }

            // Get document width or height
            if (elem.nodeType === 9) {
                doc = elem.documentElement;

                // Either scroll[Width/Height] or offset[Width/Height] or client[Width/Height], whichever is greatest
                // unfortunately, this causes bug #3838 in IE6/8 only, but there is currently no good, small way to fix it.
                return Math.max(
                    elem.body["scroll" + name], doc["scroll" + name],
                    elem.body["offset" + name], doc["offset" + name],
                    doc["client" + name]
                );
            }

            return value === undefined ?
                // Get width or height on the element, requesting but not forcing parseFloat
                jQuery.css(elem, type, value, extra) :

                // Set width or height on the element
                jQuery.style(elem, type, value, extra);
        }, type, chainable ? margin : undefined, chainable, null);
    };
    jQuery.prototype.hide = function (speed, easing, callback) {
        /// <summary>
        ///     Hide the matched elements.
        ///     &#10;1 - hide() 
        ///     &#10;2 - hide(duration, callback) 
        ///     &#10;3 - hide(duration, easing, callback)
        /// </summary>
        /// <param name="speed" type="Number">
        ///     A string or number determining how long the animation will run.
        /// </param>
        /// <param name="easing" type="String">
        ///     A string indicating which easing function to use for the transition.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A function to call once the animation is complete.
        /// </param>
        /// <returns type="jQuery" />

        return speed == null || typeof speed === "boolean" ||
            // special check for .toggle( handler, handler, ... )
            (!i && jQuery.isFunction(speed) && jQuery.isFunction(easing)) ?
            cssFn.apply(this, arguments) :
            this.animate(genFx(name, true), speed, easing, callback);
    };
    jQuery.prototype.hover = function (fnOver, fnOut) {
        /// <summary>
        ///     1: Bind two handlers to the matched elements, to be executed when the mouse pointer enters and leaves the elements.
        ///     &#10;    1.1 - hover(handlerIn(eventObject), handlerOut(eventObject))
        ///     &#10;2: Bind a single handler to the matched elements, to be executed when the mouse pointer enters or leaves the elements.
        ///     &#10;    2.1 - hover(handlerInOut(eventObject))
        /// </summary>
        /// <param name="fnOver" type="Function">
        ///     A function to execute when the mouse pointer enters the element.
        /// </param>
        /// <param name="fnOut" type="Function">
        ///     A function to execute when the mouse pointer leaves the element.
        /// </param>
        /// <returns type="jQuery" />

        return this.mouseenter(fnOver).mouseleave(fnOut || fnOver);
    };
    jQuery.prototype.html = function (value) {
        /// <summary>
        ///     1: Get the HTML contents of the first element in the set of matched elements.
        ///     &#10;    1.1 - html()
        ///     &#10;2: Set the HTML contents of each element in the set of matched elements.
        ///     &#10;    2.1 - html(htmlString) 
        ///     &#10;    2.2 - html(function(index, oldhtml))
        /// </summary>
        /// <param name="value" type="String">
        ///     A string of HTML to set as the content of each matched element.
        /// </param>
        /// <returns type="jQuery" />

        return jQuery.access(this, function (value) {
            var elem = this[0] || {},
                i = 0,
                l = this.length;

            if (value === undefined) {
                return elem.nodeType === 1 ?
                    elem.innerHTML.replace(rinlinejQuery, "") :
                    undefined;
            }

            // See if we can take a shortcut and just use innerHTML
            if (typeof value === "string" && !rnoInnerhtml.test(value) &&
                (jQuery.support.htmlSerialize || !rnoshimcache.test(value)) &&
                (jQuery.support.leadingWhitespace || !rleadingWhitespace.test(value)) &&
                !wrapMap[(rtagName.exec(value) || ["", ""])[1].toLowerCase()]) {

                value = value.replace(rxhtmlTag, "<$1></$2>");

                try {
                    for (; i < l; i++) {
                        // Remove element nodes and prevent memory leaks
                        elem = this[i] || {};
                        if (elem.nodeType === 1) {
                            jQuery.cleanData(elem.getElementsByTagName("*"));
                            elem.innerHTML = value;
                        }
                    }

                    elem = 0;

                    // If using innerHTML throws an exception, use the fallback method
                } catch (e) { }
            }

            if (elem) {
                this.empty().append(value);
            }
        }, null, value, arguments.length);
    };
    jQuery.prototype.index = function (elem) {
        /// <summary>
        ///     Search for a given element from among the matched elements.
        ///     &#10;1 - index() 
        ///     &#10;2 - index(selector) 
        ///     &#10;3 - index(element)
        /// </summary>
        /// <param name="elem" type="String">
        ///     A selector representing a jQuery collection in which to look for an element.
        /// </param>
        /// <returns type="Number" />


        // No argument, return index in parent
        if (!elem) {
            return (this[0] && this[0].parentNode) ? this.prevAll().length : -1;
        }

        // index in selector
        if (typeof elem === "string") {
            return jQuery.inArray(this[0], jQuery(elem));
        }

        // Locate the position of the desired element
        return jQuery.inArray(
            // If it receives a jQuery object, the first element is used
            elem.jquery ? elem[0] : elem, this);
    };
    jQuery.prototype.init = function (selector, context, rootjQuery) {

        var match, elem, ret, doc;

        // Handle $(""), $(null), $(undefined), $(false)
        if (!selector) {
            return this;
        }

        // Handle $(DOMElement)
        if (selector.nodeType) {
            this.context = this[0] = selector;
            this.length = 1;
            return this;
        }

        // Handle HTML strings
        if (typeof selector === "string") {
            if (selector.charAt(0) === "<" && selector.charAt(selector.length - 1) === ">" && selector.length >= 3) {
                // Assume that strings that start and end with <> are HTML and skip the regex check
                match = [null, selector, null];

            } else {
                match = rquickExpr.exec(selector);
            }

            // Match html or make sure no context is specified for #id
            if (match && (match[1] || !context)) {

                // HANDLE: $(html) -> $(array)
                if (match[1]) {
                    context = context instanceof jQuery ? context[0] : context;
                    doc = (context && context.nodeType ? context.ownerDocument || context : document);

                    // scripts is true for back-compat
                    selector = jQuery.parseHTML(match[1], doc, true);
                    if (rsingleTag.test(match[1]) && jQuery.isPlainObject(context)) {
                        this.attr.call(selector, context, true);
                    }

                    return jQuery.merge(this, selector);

                    // HANDLE: $(#id)
                } else {
                    elem = document.getElementById(match[2]);

                    // Check parentNode to catch when Blackberry 4.6 returns
                    // nodes that are no longer in the document #6963
                    if (elem && elem.parentNode) {
                        // Handle the case where IE and Opera return items
                        // by name instead of ID
                        if (elem.id !== match[2]) {
                            return rootjQuery.find(selector);
                        }

                        // Otherwise, we inject the element directly into the jQuery object
                        this.length = 1;
                        this[0] = elem;
                    }

                    this.context = document;
                    this.selector = selector;
                    return this;
                }

                // HANDLE: $(expr, $(...))
            } else if (!context || context.jquery) {
                return (context || rootjQuery).find(selector);

                // HANDLE: $(expr, context)
                // (which is just equivalent to: $(context).find(expr)
            } else {
                return this.constructor(context).find(selector);
            }

            // HANDLE: $(function)
            // Shortcut for document ready
        } else if (jQuery.isFunction(selector)) {
            return rootjQuery.ready(selector);
        }

        if (selector.selector !== undefined) {
            this.selector = selector.selector;
            this.context = selector.context;
        }

        return jQuery.makeArray(selector, this);
    };
    jQuery.prototype.innerHeight = function (margin, value) {
        /// <summary>
        ///     Get the current computed height for the first element in the set of matched elements, including padding but not border.
        /// </summary>
        /// <returns type="Number" />

        var chainable = arguments.length && (defaultExtra || typeof margin !== "boolean"),
            extra = defaultExtra || (margin === true || value === true ? "margin" : "border");

        return jQuery.access(this, function (elem, type, value) {
            var doc;

            if (jQuery.isWindow(elem)) {
                // As of 5/8/2012 this will yield incorrect results for Mobile Safari, but there
                // isn't a whole lot we can do. See pull request at this URL for discussion:
                // https://github.com/jquery/jquery/pull/764
                return elem.document.documentElement["client" + name];
            }

            // Get document width or height
            if (elem.nodeType === 9) {
                doc = elem.documentElement;

                // Either scroll[Width/Height] or offset[Width/Height] or client[Width/Height], whichever is greatest
                // unfortunately, this causes bug #3838 in IE6/8 only, but there is currently no good, small way to fix it.
                return Math.max(
                    elem.body["scroll" + name], doc["scroll" + name],
                    elem.body["offset" + name], doc["offset" + name],
                    doc["client" + name]
                );
            }

            return value === undefined ?
                // Get width or height on the element, requesting but not forcing parseFloat
                jQuery.css(elem, type, value, extra) :

                // Set width or height on the element
                jQuery.style(elem, type, value, extra);
        }, type, chainable ? margin : undefined, chainable, null);
    };
    jQuery.prototype.innerWidth = function (margin, value) {
        /// <summary>
        ///     Get the current computed width for the first element in the set of matched elements, including padding but not border.
        /// </summary>
        /// <returns type="Number" />

        var chainable = arguments.length && (defaultExtra || typeof margin !== "boolean"),
            extra = defaultExtra || (margin === true || value === true ? "margin" : "border");

        return jQuery.access(this, function (elem, type, value) {
            var doc;

            if (jQuery.isWindow(elem)) {
                // As of 5/8/2012 this will yield incorrect results for Mobile Safari, but there
                // isn't a whole lot we can do. See pull request at this URL for discussion:
                // https://github.com/jquery/jquery/pull/764
                return elem.document.documentElement["client" + name];
            }

            // Get document width or height
            if (elem.nodeType === 9) {
                doc = elem.documentElement;

                // Either scroll[Width/Height] or offset[Width/Height] or client[Width/Height], whichever is greatest
                // unfortunately, this causes bug #3838 in IE6/8 only, but there is currently no good, small way to fix it.
                return Math.max(
                    elem.body["scroll" + name], doc["scroll" + name],
                    elem.body["offset" + name], doc["offset" + name],
                    doc["client" + name]
                );
            }

            return value === undefined ?
                // Get width or height on the element, requesting but not forcing parseFloat
                jQuery.css(elem, type, value, extra) :

                // Set width or height on the element
                jQuery.style(elem, type, value, extra);
        }, type, chainable ? margin : undefined, chainable, null);
    };
    jQuery.prototype.insertAfter = function (selector) {
        /// <summary>
        ///     Insert every element in the set of matched elements after the target.
        /// </summary>
        /// <param name="selector" type="jQuery">
        ///     A selector, element, HTML string, or jQuery object; the matched set of elements will be inserted after the element(s) specified by this parameter.
        /// </param>
        /// <returns type="jQuery" />

        var elems,
            i = 0,
            ret = [],
            insert = jQuery(selector),
            l = insert.length,
            parent = this.length === 1 && this[0].parentNode;

        if ((parent == null || parent && parent.nodeType === 11 && parent.childNodes.length === 1) && l === 1) {
            insert[original](this[0]);
            return this;
        } else {
            for (; i < l; i++) {
                elems = (i > 0 ? this.clone(true) : this).get();
                jQuery(insert[i])[original](elems);
                ret = ret.concat(elems);
            }

            return this.pushStack(ret, name, insert.selector);
        }
    };
    jQuery.prototype.insertBefore = function (selector) {
        /// <summary>
        ///     Insert every element in the set of matched elements before the target.
        /// </summary>
        /// <param name="selector" type="jQuery">
        ///     A selector, element, HTML string, or jQuery object; the matched set of elements will be inserted before the element(s) specified by this parameter.
        /// </param>
        /// <returns type="jQuery" />

        var elems,
            i = 0,
            ret = [],
            insert = jQuery(selector),
            l = insert.length,
            parent = this.length === 1 && this[0].parentNode;

        if ((parent == null || parent && parent.nodeType === 11 && parent.childNodes.length === 1) && l === 1) {
            insert[original](this[0]);
            return this;
        } else {
            for (; i < l; i++) {
                elems = (i > 0 ? this.clone(true) : this).get();
                jQuery(insert[i])[original](elems);
                ret = ret.concat(elems);
            }

            return this.pushStack(ret, name, insert.selector);
        }
    };
    jQuery.prototype.is = function (selector) {
        /// <summary>
        ///     Check the current matched set of elements against a selector, element, or jQuery object and return true if at least one of these elements matches the given arguments.
        ///     &#10;1 - is(selector) 
        ///     &#10;2 - is(function(index)) 
        ///     &#10;3 - is(jQuery object) 
        ///     &#10;4 - is(element)
        /// </summary>
        /// <param name="selector" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="Boolean" />

        return !!selector && (
            typeof selector === "string" ?
                // If this is a positional/relative selector, check membership in the returned set
                // so $("p:first").is("p:last") won't return true for a doc with two "p".
                rneedsContext.test(selector) ?
                    jQuery(selector, this.context).index(this[0]) >= 0 :
                    jQuery.filter(selector, this).length > 0 :
                this.filter(selector).length > 0);
    };
    jQuery.prototype.keydown = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "keydown" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - keydown(handler(eventObject)) 
        ///     &#10;2 - keydown(eventData, handler(eventObject)) 
        ///     &#10;3 - keydown()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.keypress = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "keypress" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - keypress(handler(eventObject)) 
        ///     &#10;2 - keypress(eventData, handler(eventObject)) 
        ///     &#10;3 - keypress()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.keyup = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "keyup" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - keyup(handler(eventObject)) 
        ///     &#10;2 - keyup(eventData, handler(eventObject)) 
        ///     &#10;3 - keyup()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.last = function () {
        /// <summary>
        ///     Reduce the set of matched elements to the final one in the set.
        /// </summary>
        /// <returns type="jQuery" />

        return this.eq(-1);
    };
    jQuery.prototype.length = 0;
    jQuery.prototype.live = function (types, data, fn) {
        /// <summary>
        ///     Attach an event handler for all elements which match the current selector, now and in the future.
        ///     &#10;1 - live(events, handler(eventObject)) 
        ///     &#10;2 - live(events, data, handler(eventObject)) 
        ///     &#10;3 - live(events-map)
        /// </summary>
        /// <param name="types" type="String">
        ///     A string containing a JavaScript event type, such as "click" or "keydown." As of jQuery 1.4 the string can contain multiple, space-separated event types or custom event names.
        /// </param>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute at the time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        jQuery(this.context).on(types, this.selector, data, fn);
        return this;
    };
    jQuery.prototype.load = function (url, params, callback) {
        /// <summary>
        ///     1: Bind an event handler to the "load" JavaScript event.
        ///     &#10;    1.1 - load(handler(eventObject)) 
        ///     &#10;    1.2 - load(eventData, handler(eventObject))
        ///     &#10;2: Load data from the server and place the returned HTML into the matched element.
        ///     &#10;    2.1 - load(url, data, complete(responseText, textStatus, XMLHttpRequest))
        /// </summary>
        /// <param name="url" type="String">
        ///     A string containing the URL to which the request is sent.
        /// </param>
        /// <param name="params" type="String">
        ///     A map or string that is sent to the server with the request.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A callback function that is executed when the request completes.
        /// </param>
        /// <returns type="jQuery" />

        if (typeof url !== "string" && _load) {
            return _load.apply(this, arguments);
        }

        // Don't do a request if no elements are being requested
        if (!this.length) {
            return this;
        }

        var selector, type, response,
            self = this,
            off = url.indexOf(" ");

        if (off >= 0) {
            selector = url.slice(off, url.length);
            url = url.slice(0, off);
        }

        // If it's a function
        if (jQuery.isFunction(params)) {

            // We assume that it's the callback
            callback = params;
            params = undefined;

            // Otherwise, build a param string
        } else if (params && typeof params === "object") {
            type = "POST";
        }

        // Request the remote document
        jQuery.ajax({
            url: url,

            // if "type" variable is undefined, then "GET" method will be used
            type: type,
            dataType: "html",
            data: params,
            complete: function (jqXHR, status) {
                if (callback) {
                    self.each(callback, response || [jqXHR.responseText, status, jqXHR]);
                }
            }
        }).done(function (responseText) {

            // Save response for use in complete callback
            response = arguments;

            // See if a selector was specified
            self.html(selector ?

                // Create a dummy div to hold the results
                jQuery("<div>")

                    // inject the contents of the document in, removing the scripts
                    // to avoid any 'Permission Denied' errors in IE
                    .append(responseText.replace(rscript, ""))

                    // Locate the specified elements
                    .find(selector) :

                // If not, just inject the full result
                responseText);

        });

        return this;
    };
    jQuery.prototype.map = function (callback) {
        /// <summary>
        ///     Pass each element in the current matched set through a function, producing a new jQuery object containing the return values.
        /// </summary>
        /// <param name="callback" type="Function">
        ///     A function object that will be invoked for each element in the current set.
        /// </param>
        /// <returns type="jQuery" />

        return this.pushStack(jQuery.map(this, function (elem, i) {
            return callback.call(elem, i, elem);
        }));
    };
    jQuery.prototype.mousedown = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "mousedown" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - mousedown(handler(eventObject)) 
        ///     &#10;2 - mousedown(eventData, handler(eventObject)) 
        ///     &#10;3 - mousedown()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.mouseenter = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to be fired when the mouse enters an element, or trigger that handler on an element.
        ///     &#10;1 - mouseenter(handler(eventObject)) 
        ///     &#10;2 - mouseenter(eventData, handler(eventObject)) 
        ///     &#10;3 - mouseenter()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.mouseleave = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to be fired when the mouse leaves an element, or trigger that handler on an element.
        ///     &#10;1 - mouseleave(handler(eventObject)) 
        ///     &#10;2 - mouseleave(eventData, handler(eventObject)) 
        ///     &#10;3 - mouseleave()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.mousemove = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "mousemove" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - mousemove(handler(eventObject)) 
        ///     &#10;2 - mousemove(eventData, handler(eventObject)) 
        ///     &#10;3 - mousemove()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.mouseout = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "mouseout" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - mouseout(handler(eventObject)) 
        ///     &#10;2 - mouseout(eventData, handler(eventObject)) 
        ///     &#10;3 - mouseout()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.mouseover = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "mouseover" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - mouseover(handler(eventObject)) 
        ///     &#10;2 - mouseover(eventData, handler(eventObject)) 
        ///     &#10;3 - mouseover()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.mouseup = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "mouseup" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - mouseup(handler(eventObject)) 
        ///     &#10;2 - mouseup(eventData, handler(eventObject)) 
        ///     &#10;3 - mouseup()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.next = function (until, selector) {
        /// <summary>
        ///     Get the immediately following sibling of each element in the set of matched elements. If a selector is provided, it retrieves the next sibling only if it matches that selector.
        /// </summary>
        /// <param name="until" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.nextAll = function (until, selector) {
        /// <summary>
        ///     Get all following siblings of each element in the set of matched elements, optionally filtered by a selector.
        /// </summary>
        /// <param name="until" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.nextUntil = function (until, selector) {
        /// <summary>
        ///     Get all following siblings of each element up to but not including the element matched by the selector, DOM node, or jQuery object passed.
        ///     &#10;1 - nextUntil(selector, filter) 
        ///     &#10;2 - nextUntil(element, filter)
        /// </summary>
        /// <param name="until" type="String">
        ///     A string containing a selector expression to indicate where to stop matching following sibling elements.
        /// </param>
        /// <param name="selector" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.not = function (selector) {
        /// <summary>
        ///     Remove elements from the set of matched elements.
        ///     &#10;1 - not(selector) 
        ///     &#10;2 - not(elements) 
        ///     &#10;3 - not(function(index)) 
        ///     &#10;4 - not(jQuery object)
        /// </summary>
        /// <param name="selector" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        return this.pushStack(winnow(this, selector, false), "not", selector);
    };
    jQuery.prototype.off = function (types, selector, fn) {
        /// <summary>
        ///     Remove an event handler.
        ///     &#10;1 - off(events, selector, handler(eventObject)) 
        ///     &#10;2 - off(events-map, selector)
        /// </summary>
        /// <param name="types" type="String">
        ///     One or more space-separated event types and optional namespaces, or just namespaces, such as "click", "keydown.myPlugin", or ".myPlugin".
        /// </param>
        /// <param name="selector" type="String">
        ///     A selector which should match the one originally passed to .on() when attaching event handlers.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A handler function previously attached for the event(s), or the special value false.
        /// </param>
        /// <returns type="jQuery" />

        var handleObj, type;
        if (types && types.preventDefault && types.handleObj) {
            // ( event )  dispatched jQuery.Event
            handleObj = types.handleObj;
            jQuery(types.delegateTarget).off(
                handleObj.namespace ? handleObj.origType + "." + handleObj.namespace : handleObj.origType,
                handleObj.selector,
                handleObj.handler
            );
            return this;
        }
        if (typeof types === "object") {
            // ( types-object [, selector] )
            for (type in types) {
                this.off(type, selector, types[type]);
            }
            return this;
        }
        if (selector === false || typeof selector === "function") {
            // ( types [, fn] )
            fn = selector;
            selector = undefined;
        }
        if (fn === false) {
            fn = returnFalse;
        }
        return this.each(function () {
            jQuery.event.remove(this, types, fn, selector);
        });
    };
    jQuery.prototype.offset = function (options) {
        /// <summary>
        ///     1: Get the current coordinates of the first element in the set of matched elements, relative to the document.
        ///     &#10;    1.1 - offset()
        ///     &#10;2: Set the current coordinates of every element in the set of matched elements, relative to the document.
        ///     &#10;    2.1 - offset(coordinates) 
        ///     &#10;    2.2 - offset(function(index, coords))
        /// </summary>
        /// <param name="options" type="Object">
        ///     An object containing the properties top and left, which are integers indicating the new top and left coordinates for the elements.
        /// </param>
        /// <returns type="jQuery" />

        if (arguments.length) {
            return options === undefined ?
                this :
                this.each(function (i) {
                    jQuery.offset.setOffset(this, options, i);
                });
        }

        var box, docElem, body, win, clientTop, clientLeft, scrollTop, scrollLeft, top, left,
            elem = this[0],
            doc = elem && elem.ownerDocument;

        if (!doc) {
            return;
        }

        if ((body = doc.body) === elem) {
            return jQuery.offset.bodyOffset(elem);
        }

        docElem = doc.documentElement;

        // Make sure we're not dealing with a disconnected DOM node
        if (!jQuery.contains(docElem, elem)) {
            return { top: 0, left: 0 };
        }

        box = elem.getBoundingClientRect();
        win = getWindow(doc);
        clientTop = docElem.clientTop || body.clientTop || 0;
        clientLeft = docElem.clientLeft || body.clientLeft || 0;
        scrollTop = win.pageYOffset || docElem.scrollTop;
        scrollLeft = win.pageXOffset || docElem.scrollLeft;
        top = box.top + scrollTop - clientTop;
        left = box.left + scrollLeft - clientLeft;

        return { top: top, left: left };
    };
    jQuery.prototype.offsetParent = function () {
        /// <summary>
        ///     Get the closest ancestor element that is positioned.
        /// </summary>
        /// <returns type="jQuery" />

        return this.map(function () {
            var offsetParent = this.offsetParent || document.body;
            while (offsetParent && (!rroot.test(offsetParent.nodeName) && jQuery.css(offsetParent, "position") === "static")) {
                offsetParent = offsetParent.offsetParent;
            }
            return offsetParent || document.body;
        });
    };
    jQuery.prototype.on = function (types, selector, data, fn, /*INTERNAL*/ one) {
        /// <summary>
        ///     Attach an event handler function for one or more events to the selected elements.
        ///     &#10;1 - on(events, selector, data, handler(eventObject)) 
        ///     &#10;2 - on(events-map, selector, data)
        /// </summary>
        /// <param name="types" type="String">
        ///     One or more space-separated event types and optional namespaces, such as "click" or "keydown.myPlugin".
        /// </param>
        /// <param name="selector" type="String">
        ///     A selector string to filter the descendants of the selected elements that trigger the event. If the selector is null or omitted, the event is always triggered when it reaches the selected element.
        /// </param>
        /// <param name="data" type="Anything">
        ///     Data to be passed to the handler in event.data when an event is triggered.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute when the event is triggered. The value false is also allowed as a shorthand for a function that simply does return false.
        /// </param>
        /// <returns type="jQuery" />

        var origFn, type;

        // Types can be a map of types/handlers
        if (typeof types === "object") {
            // ( types-Object, selector, data )
            if (typeof selector !== "string") { // && selector != null
                // ( types-Object, data )
                data = data || selector;
                selector = undefined;
            }
            for (type in types) {
                this.on(type, selector, data, types[type], one);
            }
            return this;
        }

        if (data == null && fn == null) {
            // ( types, fn )
            fn = selector;
            data = selector = undefined;
        } else if (fn == null) {
            if (typeof selector === "string") {
                // ( types, selector, fn )
                fn = data;
                data = undefined;
            } else {
                // ( types, data, fn )
                fn = data;
                data = selector;
                selector = undefined;
            }
        }
        if (fn === false) {
            fn = returnFalse;
        } else if (!fn) {
            return this;
        }

        if (one === 1) {
            origFn = fn;
            fn = function (event) {
                // Can use an empty set, since event contains the info
                jQuery().off(event);
                return origFn.apply(this, arguments);
            };
            // Use same guid so caller can remove using origFn
            fn.guid = origFn.guid || (origFn.guid = jQuery.guid++);
        }
        return this.each(function () {
            jQuery.event.add(this, types, fn, data, selector);
        });
    };
    jQuery.prototype.one = function (types, selector, data, fn) {
        /// <summary>
        ///     Attach a handler to an event for the elements. The handler is executed at most once per element.
        ///     &#10;1 - one(events, data, handler(eventObject)) 
        ///     &#10;2 - one(events, selector, data, handler(eventObject)) 
        ///     &#10;3 - one(events-map, selector, data)
        /// </summary>
        /// <param name="types" type="String">
        ///     One or more space-separated event types and optional namespaces, such as "click" or "keydown.myPlugin".
        /// </param>
        /// <param name="selector" type="String">
        ///     A selector string to filter the descendants of the selected elements that trigger the event. If the selector is null or omitted, the event is always triggered when it reaches the selected element.
        /// </param>
        /// <param name="data" type="Anything">
        ///     Data to be passed to the handler in event.data when an event is triggered.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute when the event is triggered. The value false is also allowed as a shorthand for a function that simply does return false.
        /// </param>
        /// <returns type="jQuery" />

        return this.on(types, selector, data, fn, 1);
    };
    jQuery.prototype.outerHeight = function (margin, value) {
        /// <summary>
        ///     Get the current computed height for the first element in the set of matched elements, including padding, border, and optionally margin. Returns an integer (without "px") representation of the value or null if called on an empty set of elements.
        /// </summary>
        /// <param name="margin" type="Boolean">
        ///     A Boolean indicating whether to include the element's margin in the calculation.
        /// </param>
        /// <returns type="Number" />

        var chainable = arguments.length && (defaultExtra || typeof margin !== "boolean"),
            extra = defaultExtra || (margin === true || value === true ? "margin" : "border");

        return jQuery.access(this, function (elem, type, value) {
            var doc;

            if (jQuery.isWindow(elem)) {
                // As of 5/8/2012 this will yield incorrect results for Mobile Safari, but there
                // isn't a whole lot we can do. See pull request at this URL for discussion:
                // https://github.com/jquery/jquery/pull/764
                return elem.document.documentElement["client" + name];
            }

            // Get document width or height
            if (elem.nodeType === 9) {
                doc = elem.documentElement;

                // Either scroll[Width/Height] or offset[Width/Height] or client[Width/Height], whichever is greatest
                // unfortunately, this causes bug #3838 in IE6/8 only, but there is currently no good, small way to fix it.
                return Math.max(
                    elem.body["scroll" + name], doc["scroll" + name],
                    elem.body["offset" + name], doc["offset" + name],
                    doc["client" + name]
                );
            }

            return value === undefined ?
                // Get width or height on the element, requesting but not forcing parseFloat
                jQuery.css(elem, type, value, extra) :

                // Set width or height on the element
                jQuery.style(elem, type, value, extra);
        }, type, chainable ? margin : undefined, chainable, null);
    };
    jQuery.prototype.outerWidth = function (margin, value) {
        /// <summary>
        ///     Get the current computed width for the first element in the set of matched elements, including padding and border.
        /// </summary>
        /// <param name="margin" type="Boolean">
        ///     A Boolean indicating whether to include the element's margin in the calculation.
        /// </param>
        /// <returns type="Number" />

        var chainable = arguments.length && (defaultExtra || typeof margin !== "boolean"),
            extra = defaultExtra || (margin === true || value === true ? "margin" : "border");

        return jQuery.access(this, function (elem, type, value) {
            var doc;

            if (jQuery.isWindow(elem)) {
                // As of 5/8/2012 this will yield incorrect results for Mobile Safari, but there
                // isn't a whole lot we can do. See pull request at this URL for discussion:
                // https://github.com/jquery/jquery/pull/764
                return elem.document.documentElement["client" + name];
            }

            // Get document width or height
            if (elem.nodeType === 9) {
                doc = elem.documentElement;

                // Either scroll[Width/Height] or offset[Width/Height] or client[Width/Height], whichever is greatest
                // unfortunately, this causes bug #3838 in IE6/8 only, but there is currently no good, small way to fix it.
                return Math.max(
                    elem.body["scroll" + name], doc["scroll" + name],
                    elem.body["offset" + name], doc["offset" + name],
                    doc["client" + name]
                );
            }

            return value === undefined ?
                // Get width or height on the element, requesting but not forcing parseFloat
                jQuery.css(elem, type, value, extra) :

                // Set width or height on the element
                jQuery.style(elem, type, value, extra);
        }, type, chainable ? margin : undefined, chainable, null);
    };
    jQuery.prototype.parent = function (until, selector) {
        /// <summary>
        ///     Get the parent of each element in the current set of matched elements, optionally filtered by a selector.
        /// </summary>
        /// <param name="until" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.parents = function (until, selector) {
        /// <summary>
        ///     Get the ancestors of each element in the current set of matched elements, optionally filtered by a selector.
        /// </summary>
        /// <param name="until" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.parentsUntil = function (until, selector) {
        /// <summary>
        ///     Get the ancestors of each element in the current set of matched elements, up to but not including the element matched by the selector, DOM node, or jQuery object.
        ///     &#10;1 - parentsUntil(selector, filter) 
        ///     &#10;2 - parentsUntil(element, filter)
        /// </summary>
        /// <param name="until" type="String">
        ///     A string containing a selector expression to indicate where to stop matching ancestor elements.
        /// </param>
        /// <param name="selector" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.position = function () {
        /// <summary>
        ///     Get the current coordinates of the first element in the set of matched elements, relative to the offset parent.
        /// </summary>
        /// <returns type="Object" />

        if (!this[0]) {
            return;
        }

        var elem = this[0],

        // Get *real* offsetParent
        offsetParent = this.offsetParent(),

        // Get correct offsets
        offset = this.offset(),
        parentOffset = rroot.test(offsetParent[0].nodeName) ? { top: 0, left: 0 } : offsetParent.offset();

        // Subtract element margins
        // note: when an element has margin: auto the offsetLeft and marginLeft
        // are the same in Safari causing offset.left to incorrectly be 0
        offset.top -= parseFloat(jQuery.css(elem, "marginTop")) || 0;
        offset.left -= parseFloat(jQuery.css(elem, "marginLeft")) || 0;

        // Add offsetParent borders
        parentOffset.top += parseFloat(jQuery.css(offsetParent[0], "borderTopWidth")) || 0;
        parentOffset.left += parseFloat(jQuery.css(offsetParent[0], "borderLeftWidth")) || 0;

        // Subtract the two offsets
        return {
            top: offset.top - parentOffset.top,
            left: offset.left - parentOffset.left
        };
    };
    jQuery.prototype.prepend = function () {
        /// <summary>
        ///     Insert content, specified by the parameter, to the beginning of each element in the set of matched elements.
        ///     &#10;1 - prepend(content, content) 
        ///     &#10;2 - prepend(function(index, html))
        /// </summary>
        /// <param name="" type="jQuery">
        ///     DOM element, array of elements, HTML string, or jQuery object to insert at the beginning of each element in the set of matched elements.
        /// </param>
        /// <param name="" type="jQuery">
        ///     One or more additional DOM elements, arrays of elements, HTML strings, or jQuery objects to insert at the beginning of each element in the set of matched elements.
        /// </param>
        /// <returns type="jQuery" />

        return this.domManip(arguments, true, function (elem) {
            if (this.nodeType === 1 || this.nodeType === 11) {
                this.insertBefore(elem, this.firstChild);
            }
        });
    };
    jQuery.prototype.prependTo = function (selector) {
        /// <summary>
        ///     Insert every element in the set of matched elements to the beginning of the target.
        /// </summary>
        /// <param name="selector" type="jQuery">
        ///     A selector, element, HTML string, or jQuery object; the matched set of elements will be inserted at the beginning of the element(s) specified by this parameter.
        /// </param>
        /// <returns type="jQuery" />

        var elems,
            i = 0,
            ret = [],
            insert = jQuery(selector),
            l = insert.length,
            parent = this.length === 1 && this[0].parentNode;

        if ((parent == null || parent && parent.nodeType === 11 && parent.childNodes.length === 1) && l === 1) {
            insert[original](this[0]);
            return this;
        } else {
            for (; i < l; i++) {
                elems = (i > 0 ? this.clone(true) : this).get();
                jQuery(insert[i])[original](elems);
                ret = ret.concat(elems);
            }

            return this.pushStack(ret, name, insert.selector);
        }
    };
    jQuery.prototype.prev = function (until, selector) {
        /// <summary>
        ///     Get the immediately preceding sibling of each element in the set of matched elements, optionally filtered by a selector.
        /// </summary>
        /// <param name="until" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.prevAll = function (until, selector) {
        /// <summary>
        ///     Get all preceding siblings of each element in the set of matched elements, optionally filtered by a selector.
        /// </summary>
        /// <param name="until" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.prevUntil = function (until, selector) {
        /// <summary>
        ///     Get all preceding siblings of each element up to but not including the element matched by the selector, DOM node, or jQuery object.
        ///     &#10;1 - prevUntil(selector, filter) 
        ///     &#10;2 - prevUntil(element, filter)
        /// </summary>
        /// <param name="until" type="String">
        ///     A string containing a selector expression to indicate where to stop matching preceding sibling elements.
        /// </param>
        /// <param name="selector" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.promise = function (type, obj) {
        /// <summary>
        ///     Return a Promise object to observe when all actions of a certain type bound to the collection, queued or not, have finished.
        /// </summary>
        /// <param name="type" type="String">
        ///     The type of queue that needs to be observed.
        /// </param>
        /// <param name="obj" type="Object">
        ///     Object onto which the promise methods have to be attached
        /// </param>
        /// <returns type="Promise" />

        var tmp,
            count = 1,
            defer = jQuery.Deferred(),
            elements = this,
            i = this.length,
            resolve = function () {
                if (!(--count)) {
                    defer.resolveWith(elements, [elements]);
                }
            };

        if (typeof type !== "string") {
            obj = type;
            type = undefined;
        }
        type = type || "fx";

        while (i--) {
            tmp = jQuery._data(elements[i], type + "queueHooks");
            if (tmp && tmp.empty) {
                count++;
                tmp.empty.add(resolve);
            }
        }
        resolve();
        return defer.promise(obj);
    };
    jQuery.prototype.prop = function (name, value) {
        /// <summary>
        ///     1: Get the value of a property for the first element in the set of matched elements.
        ///     &#10;    1.1 - prop(propertyName)
        ///     &#10;2: Set one or more properties for the set of matched elements.
        ///     &#10;    2.1 - prop(propertyName, value) 
        ///     &#10;    2.2 - prop(map) 
        ///     &#10;    2.3 - prop(propertyName, function(index, oldPropertyValue))
        /// </summary>
        /// <param name="name" type="String">
        ///     The name of the property to set.
        /// </param>
        /// <param name="value" type="Boolean">
        ///     A value to set for the property.
        /// </param>
        /// <returns type="jQuery" />

        return jQuery.access(this, jQuery.prop, name, value, arguments.length > 1);
    };
    jQuery.prototype.pushStack = function (elems, name, selector) {
        /// <summary>
        ///     Add a collection of DOM elements onto the jQuery stack.
        ///     &#10;1 - pushStack(elements) 
        ///     &#10;2 - pushStack(elements, name, arguments)
        /// </summary>
        /// <param name="elems" type="Array">
        ///     An array of elements to push onto the stack and make into a new jQuery object.
        /// </param>
        /// <param name="name" type="String">
        ///     The name of a jQuery method that generated the array of elements.
        /// </param>
        /// <param name="selector" type="Array">
        ///     The arguments that were passed in to the jQuery method (for serialization).
        /// </param>
        /// <returns type="jQuery" />


        // Build a new jQuery matched element set
        var ret = jQuery.merge(this.constructor(), elems);

        // Add the old object onto the stack (as a reference)
        ret.prevObject = this;

        ret.context = this.context;

        if (name === "find") {
            ret.selector = this.selector + (this.selector ? " " : "") + selector;
        } else if (name) {
            ret.selector = this.selector + "." + name + "(" + selector + ")";
        }

        // Return the newly-formed element set
        return ret;
    };
    jQuery.prototype.queue = function (type, data) {
        /// <summary>
        ///     1: Show the queue of functions to be executed on the matched elements.
        ///     &#10;    1.1 - queue(queueName)
        ///     &#10;2: Manipulate the queue of functions to be executed on the matched elements.
        ///     &#10;    2.1 - queue(queueName, newQueue) 
        ///     &#10;    2.2 - queue(queueName, callback( next ))
        /// </summary>
        /// <param name="type" type="String">
        ///     A string containing the name of the queue. Defaults to fx, the standard effects queue.
        /// </param>
        /// <param name="data" type="Array">
        ///     An array of functions to replace the current queue contents.
        /// </param>
        /// <returns type="jQuery" />

        var setter = 2;

        if (typeof type !== "string") {
            data = type;
            type = "fx";
            setter--;
        }

        if (arguments.length < setter) {
            return jQuery.queue(this[0], type);
        }

        return data === undefined ?
            this :
            this.each(function () {
                var queue = jQuery.queue(this, type, data);

                // ensure a hooks for this queue
                jQuery._queueHooks(this, type);

                if (type === "fx" && queue[0] !== "inprogress") {
                    jQuery.dequeue(this, type);
                }
            });
    };
    jQuery.prototype.ready = function (fn) {
        /// <summary>
        ///     Specify a function to execute when the DOM is fully loaded.
        /// </summary>
        /// <param name="fn" type="Function">
        ///     A function to execute after the DOM is ready.
        /// </param>
        /// <returns type="jQuery" />

        // Add the callback
        jQuery.ready.promise().done(fn);

        return this;
    };
    jQuery.prototype.remove = function (selector, keepData) {
        /// <summary>
        ///     Remove the set of matched elements from the DOM.
        /// </summary>
        /// <param name="selector" type="String">
        ///     A selector expression that filters the set of matched elements to be removed.
        /// </param>
        /// <returns type="jQuery" />

        var elem,
            i = 0;

        for (; (elem = this[i]) != null; i++) {
            if (!selector || jQuery.filter(selector, [elem]).length) {
                if (!keepData && elem.nodeType === 1) {
                    jQuery.cleanData(elem.getElementsByTagName("*"));
                    jQuery.cleanData([elem]);
                }

                if (elem.parentNode) {
                    elem.parentNode.removeChild(elem);
                }
            }
        }

        return this;
    };
    jQuery.prototype.removeAttr = function (name) {
        /// <summary>
        ///     Remove an attribute from each element in the set of matched elements.
        /// </summary>
        /// <param name="name" type="String">
        ///     An attribute to remove; as of version 1.7, it can be a space-separated list of attributes.
        /// </param>
        /// <returns type="jQuery" />

        return this.each(function () {
            jQuery.removeAttr(this, name);
        });
    };
    jQuery.prototype.removeClass = function (value) {
        /// <summary>
        ///     Remove a single class, multiple classes, or all classes from each element in the set of matched elements.
        ///     &#10;1 - removeClass(className) 
        ///     &#10;2 - removeClass(function(index, class))
        /// </summary>
        /// <param name="value" type="String">
        ///     One or more space-separated classes to be removed from the class attribute of each matched element.
        /// </param>
        /// <returns type="jQuery" />

        var removes, className, elem, c, cl, i, l;

        if (jQuery.isFunction(value)) {
            return this.each(function (j) {
                jQuery(this).removeClass(value.call(this, j, this.className));
            });
        }
        if ((value && typeof value === "string") || value === undefined) {
            removes = (value || "").split(core_rspace);

            for (i = 0, l = this.length; i < l; i++) {
                elem = this[i];
                if (elem.nodeType === 1 && elem.className) {

                    className = (" " + elem.className + " ").replace(rclass, " ");

                    // loop over each item in the removal list
                    for (c = 0, cl = removes.length; c < cl; c++) {
                        // Remove until there is nothing to remove,
                        while (className.indexOf(" " + removes[c] + " ") > -1) {
                            className = className.replace(" " + removes[c] + " ", " ");
                        }
                    }
                    elem.className = value ? jQuery.trim(className) : "";
                }
            }
        }

        return this;
    };
    jQuery.prototype.removeData = function (key) {
        /// <summary>
        ///     Remove a previously-stored piece of data.
        ///     &#10;1 - removeData(name) 
        ///     &#10;2 - removeData(list)
        /// </summary>
        /// <param name="key" type="String">
        ///     A string naming the piece of data to delete.
        /// </param>
        /// <returns type="jQuery" />

        return this.each(function () {
            jQuery.removeData(this, key);
        });
    };
    jQuery.prototype.removeProp = function (name) {
        /// <summary>
        ///     Remove a property for the set of matched elements.
        /// </summary>
        /// <param name="name" type="String">
        ///     The name of the property to set.
        /// </param>
        /// <returns type="jQuery" />

        name = jQuery.propFix[name] || name;
        return this.each(function () {
            // try/catch handles cases where IE balks (such as removing a property on window)
            try {
                this[name] = undefined;
                delete this[name];
            } catch (e) { }
        });
    };
    jQuery.prototype.replaceAll = function (selector) {
        /// <summary>
        ///     Replace each target element with the set of matched elements.
        /// </summary>
        /// <param name="selector" type="String">
        ///     A selector expression indicating which element(s) to replace.
        /// </param>
        /// <returns type="jQuery" />

        var elems,
            i = 0,
            ret = [],
            insert = jQuery(selector),
            l = insert.length,
            parent = this.length === 1 && this[0].parentNode;

        if ((parent == null || parent && parent.nodeType === 11 && parent.childNodes.length === 1) && l === 1) {
            insert[original](this[0]);
            return this;
        } else {
            for (; i < l; i++) {
                elems = (i > 0 ? this.clone(true) : this).get();
                jQuery(insert[i])[original](elems);
                ret = ret.concat(elems);
            }

            return this.pushStack(ret, name, insert.selector);
        }
    };
    jQuery.prototype.replaceWith = function (value) {
        /// <summary>
        ///     Replace each element in the set of matched elements with the provided new content.
        ///     &#10;1 - replaceWith(newContent) 
        ///     &#10;2 - replaceWith(function)
        /// </summary>
        /// <param name="value" type="jQuery">
        ///     The content to insert. May be an HTML string, DOM element, or jQuery object.
        /// </param>
        /// <returns type="jQuery" />

        if (!isDisconnected(this[0])) {
            // Make sure that the elements are removed from the DOM before they are inserted
            // this can help fix replacing a parent with child elements
            if (jQuery.isFunction(value)) {
                return this.each(function (i) {
                    var self = jQuery(this), old = self.html();
                    self.replaceWith(value.call(this, i, old));
                });
            }

            if (typeof value !== "string") {
                value = jQuery(value).detach();
            }

            return this.each(function () {
                var next = this.nextSibling,
                    parent = this.parentNode;

                jQuery(this).remove();

                if (next) {
                    jQuery(next).before(value);
                } else {
                    jQuery(parent).append(value);
                }
            });
        }

        return this.length ?
            this.pushStack(jQuery(jQuery.isFunction(value) ? value() : value), "replaceWith", value) :
            this;
    };
    jQuery.prototype.resize = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "resize" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - resize(handler(eventObject)) 
        ///     &#10;2 - resize(eventData, handler(eventObject)) 
        ///     &#10;3 - resize()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.scroll = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "scroll" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - scroll(handler(eventObject)) 
        ///     &#10;2 - scroll(eventData, handler(eventObject)) 
        ///     &#10;3 - scroll()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.scrollLeft = function (val) {
        /// <summary>
        ///     1: Get the current horizontal position of the scroll bar for the first element in the set of matched elements.
        ///     &#10;    1.1 - scrollLeft()
        ///     &#10;2: Set the current horizontal position of the scroll bar for each of the set of matched elements.
        ///     &#10;    2.1 - scrollLeft(value)
        /// </summary>
        /// <param name="val" type="Number">
        ///     An integer indicating the new position to set the scroll bar to.
        /// </param>
        /// <returns type="jQuery" />

        return jQuery.access(this, function (elem, method, val) {
            var win = getWindow(elem);

            if (val === undefined) {
                return win ? (prop in win) ? win[prop] :
                    win.document.documentElement[method] :
                    elem[method];
            }

            if (win) {
                win.scrollTo(
                    !top ? val : jQuery(win).scrollLeft(),
                     top ? val : jQuery(win).scrollTop()
                );

            } else {
                elem[method] = val;
            }
        }, method, val, arguments.length, null);
    };
    jQuery.prototype.scrollTop = function (val) {
        /// <summary>
        ///     1: Get the current vertical position of the scroll bar for the first element in the set of matched elements.
        ///     &#10;    1.1 - scrollTop()
        ///     &#10;2: Set the current vertical position of the scroll bar for each of the set of matched elements.
        ///     &#10;    2.1 - scrollTop(value)
        /// </summary>
        /// <param name="val" type="Number">
        ///     An integer indicating the new position to set the scroll bar to.
        /// </param>
        /// <returns type="jQuery" />

        return jQuery.access(this, function (elem, method, val) {
            var win = getWindow(elem);

            if (val === undefined) {
                return win ? (prop in win) ? win[prop] :
                    win.document.documentElement[method] :
                    elem[method];
            }

            if (win) {
                win.scrollTo(
                    !top ? val : jQuery(win).scrollLeft(),
                     top ? val : jQuery(win).scrollTop()
                );

            } else {
                elem[method] = val;
            }
        }, method, val, arguments.length, null);
    };
    jQuery.prototype.select = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "select" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - select(handler(eventObject)) 
        ///     &#10;2 - select(eventData, handler(eventObject)) 
        ///     &#10;3 - select()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.serialize = function () {
        /// <summary>
        ///     Encode a set of form elements as a string for submission.
        /// </summary>
        /// <returns type="String" />

        return jQuery.param(this.serializeArray());
    };
    jQuery.prototype.serializeArray = function () {
        /// <summary>
        ///     Encode a set of form elements as an array of names and values.
        /// </summary>
        /// <returns type="Array" />

        return this.map(function () {
            return this.elements ? jQuery.makeArray(this.elements) : this;
        })
        .filter(function () {
            return this.name && !this.disabled &&
                (this.checked || rselectTextarea.test(this.nodeName) ||
                    rinput.test(this.type));
        })
        .map(function (i, elem) {
            var val = jQuery(this).val();

            return val == null ?
                null :
                jQuery.isArray(val) ?
                    jQuery.map(val, function (val, i) {
                        return { name: elem.name, value: val.replace(rCRLF, "\r\n") };
                    }) :
                    { name: elem.name, value: val.replace(rCRLF, "\r\n") };
        }).get();
    };
    jQuery.prototype.show = function (speed, easing, callback) {
        /// <summary>
        ///     Display the matched elements.
        ///     &#10;1 - show() 
        ///     &#10;2 - show(duration, callback) 
        ///     &#10;3 - show(duration, easing, callback)
        /// </summary>
        /// <param name="speed" type="Number">
        ///     A string or number determining how long the animation will run.
        /// </param>
        /// <param name="easing" type="String">
        ///     A string indicating which easing function to use for the transition.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A function to call once the animation is complete.
        /// </param>
        /// <returns type="jQuery" />

        return speed == null || typeof speed === "boolean" ||
            // special check for .toggle( handler, handler, ... )
            (!i && jQuery.isFunction(speed) && jQuery.isFunction(easing)) ?
            cssFn.apply(this, arguments) :
            this.animate(genFx(name, true), speed, easing, callback);
    };
    jQuery.prototype.siblings = function (until, selector) {
        /// <summary>
        ///     Get the siblings of each element in the set of matched elements, optionally filtered by a selector.
        /// </summary>
        /// <param name="until" type="String">
        ///     A string containing a selector expression to match elements against.
        /// </param>
        /// <returns type="jQuery" />

        var ret = jQuery.map(this, fn, until);

        if (!runtil.test(name)) {
            selector = until;
        }

        if (selector && typeof selector === "string") {
            ret = jQuery.filter(selector, ret);
        }

        ret = this.length > 1 && !guaranteedUnique[name] ? jQuery.unique(ret) : ret;

        if (this.length > 1 && rparentsprev.test(name)) {
            ret = ret.reverse();
        }

        return this.pushStack(ret, name, core_slice.call(arguments).join(","));
    };
    jQuery.prototype.size = function () {
        /// <summary>
        ///     Return the number of elements in the jQuery object.
        /// </summary>
        /// <returns type="Number" />

        return this.length;
    };
    jQuery.prototype.slice = function () {
        /// <summary>
        ///     Reduce the set of matched elements to a subset specified by a range of indices.
        /// </summary>
        /// <param name="" type="Number">
        ///     An integer indicating the 0-based position at which the elements begin to be selected. If negative, it indicates an offset from the end of the set.
        /// </param>
        /// <param name="" type="Number">
        ///     An integer indicating the 0-based position at which the elements stop being selected. If negative, it indicates an offset from the end of the set. If omitted, the range continues until the end of the set.
        /// </param>
        /// <returns type="jQuery" />

        return this.pushStack(core_slice.apply(this, arguments),
            "slice", core_slice.call(arguments).join(","));
    };
    jQuery.prototype.slideDown = function (speed, easing, callback) {
        /// <summary>
        ///     Display the matched elements with a sliding motion.
        ///     &#10;1 - slideDown(duration, callback) 
        ///     &#10;2 - slideDown(duration, easing, callback)
        /// </summary>
        /// <param name="speed" type="Number">
        ///     A string or number determining how long the animation will run.
        /// </param>
        /// <param name="easing" type="String">
        ///     A string indicating which easing function to use for the transition.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A function to call once the animation is complete.
        /// </param>
        /// <returns type="jQuery" />

        return this.animate(props, speed, easing, callback);
    };
    jQuery.prototype.slideToggle = function (speed, easing, callback) {
        /// <summary>
        ///     Display or hide the matched elements with a sliding motion.
        ///     &#10;1 - slideToggle(duration, callback) 
        ///     &#10;2 - slideToggle(duration, easing, callback)
        /// </summary>
        /// <param name="speed" type="Number">
        ///     A string or number determining how long the animation will run.
        /// </param>
        /// <param name="easing" type="String">
        ///     A string indicating which easing function to use for the transition.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A function to call once the animation is complete.
        /// </param>
        /// <returns type="jQuery" />

        return this.animate(props, speed, easing, callback);
    };
    jQuery.prototype.slideUp = function (speed, easing, callback) {
        /// <summary>
        ///     Hide the matched elements with a sliding motion.
        ///     &#10;1 - slideUp(duration, callback) 
        ///     &#10;2 - slideUp(duration, easing, callback)
        /// </summary>
        /// <param name="speed" type="Number">
        ///     A string or number determining how long the animation will run.
        /// </param>
        /// <param name="easing" type="String">
        ///     A string indicating which easing function to use for the transition.
        /// </param>
        /// <param name="callback" type="Function">
        ///     A function to call once the animation is complete.
        /// </param>
        /// <returns type="jQuery" />

        return this.animate(props, speed, easing, callback);
    };
    jQuery.prototype.stop = function (type, clearQueue, gotoEnd) {
        /// <summary>
        ///     Stop the currently-running animation on the matched elements.
        ///     &#10;1 - stop(clearQueue, jumpToEnd) 
        ///     &#10;2 - stop(queue, clearQueue, jumpToEnd)
        /// </summary>
        /// <param name="type" type="String">
        ///     The name of the queue in which to stop animations.
        /// </param>
        /// <param name="clearQueue" type="Boolean">
        ///     A Boolean indicating whether to remove queued animation as well. Defaults to false.
        /// </param>
        /// <param name="gotoEnd" type="Boolean">
        ///     A Boolean indicating whether to complete the current animation immediately. Defaults to false.
        /// </param>
        /// <returns type="jQuery" />

        var stopQueue = function (hooks) {
            var stop = hooks.stop;
            delete hooks.stop;
            stop(gotoEnd);
        };

        if (typeof type !== "string") {
            gotoEnd = clearQueue;
            clearQueue = type;
            type = undefined;
        }
        if (clearQueue && type !== false) {
            this.queue(type || "fx", []);
        }

        return this.each(function () {
            var dequeue = true,
                index = type != null && type + "queueHooks",
                timers = jQuery.timers,
                data = jQuery._data(this);

            if (index) {
                if (data[index] && data[index].stop) {
                    stopQueue(data[index]);
                }
            } else {
                for (index in data) {
                    if (data[index] && data[index].stop && rrun.test(index)) {
                        stopQueue(data[index]);
                    }
                }
            }

            for (index = timers.length; index--;) {
                if (timers[index].elem === this && (type == null || timers[index].queue === type)) {
                    timers[index].anim.stop(gotoEnd);
                    dequeue = false;
                    timers.splice(index, 1);
                }
            }

            // start the next in the queue if the last step wasn't forced
            // timers currently will call their complete callbacks, which will dequeue
            // but only if they were gotoEnd
            if (dequeue || !gotoEnd) {
                jQuery.dequeue(this, type);
            }
        });
    };
    jQuery.prototype.submit = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "submit" JavaScript event, or trigger that event on an element.
        ///     &#10;1 - submit(handler(eventObject)) 
        ///     &#10;2 - submit(eventData, handler(eventObject)) 
        ///     &#10;3 - submit()
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.text = function (value) {
        /// <summary>
        ///     1: Get the combined text contents of each element in the set of matched elements, including their descendants.
        ///     &#10;    1.1 - text()
        ///     &#10;2: Set the content of each element in the set of matched elements to the specified text.
        ///     &#10;    2.1 - text(textString) 
        ///     &#10;    2.2 - text(function(index, text))
        /// </summary>
        /// <param name="value" type="String">
        ///     A string of text to set as the content of each matched element.
        /// </param>
        /// <returns type="jQuery" />

        return jQuery.access(this, function (value) {
            return value === undefined ?
                jQuery.text(this) :
                this.empty().append((this[0] && this[0].ownerDocument || document).createTextNode(value));
        }, null, value, arguments.length);
    };
    jQuery.prototype.toArray = function () {
        /// <summary>
        ///     Retrieve all the DOM elements contained in the jQuery set, as an array.
        /// </summary>
        /// <returns type="Array" />

        return core_slice.call(this);
    };
    jQuery.prototype.toggle = function (speed, easing, callback) {
        /// <summary>
        ///     1: Bind two or more handlers to the matched elements, to be executed on alternate clicks.
        ///     &#10;    1.1 - toggle(handler(eventObject), handler(eventObject), handler(eventObject))
        ///     &#10;2: Display or hide the matched elements.
        ///     &#10;    2.1 - toggle(duration, callback) 
        ///     &#10;    2.2 - toggle(duration, easing, callback) 
        ///     &#10;    2.3 - toggle(showOrHide)
        /// </summary>
        /// <param name="speed" type="Function">
        ///     A function to execute every even time the element is clicked.
        /// </param>
        /// <param name="easing" type="Function">
        ///     A function to execute every odd time the element is clicked.
        /// </param>
        /// <param name="callback" type="Function">
        ///     Additional handlers to cycle through after clicks.
        /// </param>
        /// <returns type="jQuery" />

        return speed == null || typeof speed === "boolean" ||
            // special check for .toggle( handler, handler, ... )
            (!i && jQuery.isFunction(speed) && jQuery.isFunction(easing)) ?
            cssFn.apply(this, arguments) :
            this.animate(genFx(name, true), speed, easing, callback);
    };
    jQuery.prototype.toggleClass = function (value, stateVal) {
        /// <summary>
        ///     Add or remove one or more classes from each element in the set of matched elements, depending on either the class's presence or the value of the switch argument.
        ///     &#10;1 - toggleClass(className) 
        ///     &#10;2 - toggleClass(className, switch) 
        ///     &#10;3 - toggleClass(switch) 
        ///     &#10;4 - toggleClass(function(index, class, switch), switch)
        /// </summary>
        /// <param name="value" type="String">
        ///     One or more class names (separated by spaces) to be toggled for each element in the matched set.
        /// </param>
        /// <param name="stateVal" type="Boolean">
        ///     A Boolean (not just truthy/falsy) value to determine whether the class should be added or removed.
        /// </param>
        /// <returns type="jQuery" />

        var type = typeof value,
            isBool = typeof stateVal === "boolean";

        if (jQuery.isFunction(value)) {
            return this.each(function (i) {
                jQuery(this).toggleClass(value.call(this, i, this.className, stateVal), stateVal);
            });
        }

        return this.each(function () {
            if (type === "string") {
                // toggle individual class names
                var className,
                    i = 0,
                    self = jQuery(this),
                    state = stateVal,
                    classNames = value.split(core_rspace);

                while ((className = classNames[i++])) {
                    // check each className given, space separated list
                    state = isBool ? state : !self.hasClass(className);
                    self[state ? "addClass" : "removeClass"](className);
                }

            } else if (type === "undefined" || type === "boolean") {
                if (this.className) {
                    // store className if set
                    jQuery._data(this, "__className__", this.className);
                }

                // toggle whole className
                this.className = this.className || value === false ? "" : jQuery._data(this, "__className__") || "";
            }
        });
    };
    jQuery.prototype.trigger = function (type, data) {
        /// <summary>
        ///     Execute all handlers and behaviors attached to the matched elements for the given event type.
        ///     &#10;1 - trigger(eventType, extraParameters) 
        ///     &#10;2 - trigger(event)
        /// </summary>
        /// <param name="type" type="String">
        ///     A string containing a JavaScript event type, such as click or submit.
        /// </param>
        /// <param name="data" type="Object">
        ///     Additional parameters to pass along to the event handler.
        /// </param>
        /// <returns type="jQuery" />

        return this.each(function () {
            jQuery.event.trigger(type, data, this);
        });
    };
    jQuery.prototype.triggerHandler = function (type, data) {
        /// <summary>
        ///     Execute all handlers attached to an element for an event.
        /// </summary>
        /// <param name="type" type="String">
        ///     A string containing a JavaScript event type, such as click or submit.
        /// </param>
        /// <param name="data" type="Array">
        ///     An array of additional parameters to pass along to the event handler.
        /// </param>
        /// <returns type="Object" />

        if (this[0]) {
            return jQuery.event.trigger(type, data, this[0], true);
        }
    };
    jQuery.prototype.unbind = function (types, fn) {
        /// <summary>
        ///     Remove a previously-attached event handler from the elements.
        ///     &#10;1 - unbind(eventType, handler(eventObject)) 
        ///     &#10;2 - unbind(eventType, false) 
        ///     &#10;3 - unbind(event)
        /// </summary>
        /// <param name="types" type="String">
        ///     A string containing a JavaScript event type, such as click or submit.
        /// </param>
        /// <param name="fn" type="Function">
        ///     The function that is to be no longer executed.
        /// </param>
        /// <returns type="jQuery" />

        return this.off(types, null, fn);
    };
    jQuery.prototype.undelegate = function (selector, types, fn) {
        /// <summary>
        ///     Remove a handler from the event for all elements which match the current selector, based upon a specific set of root elements.
        ///     &#10;1 - undelegate() 
        ///     &#10;2 - undelegate(selector, eventType) 
        ///     &#10;3 - undelegate(selector, eventType, handler(eventObject)) 
        ///     &#10;4 - undelegate(selector, events) 
        ///     &#10;5 - undelegate(namespace)
        /// </summary>
        /// <param name="selector" type="String">
        ///     A selector which will be used to filter the event results.
        /// </param>
        /// <param name="types" type="String">
        ///     A string containing a JavaScript event type, such as "click" or "keydown"
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute at the time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        // ( namespace ) or ( selector, types [, fn] )
        return arguments.length == 1 ? this.off(selector, "**") : this.off(types, selector || "**", fn);
    };
    jQuery.prototype.unload = function (data, fn) {
        /// <summary>
        ///     Bind an event handler to the "unload" JavaScript event.
        ///     &#10;1 - unload(handler(eventObject)) 
        ///     &#10;2 - unload(eventData, handler(eventObject))
        /// </summary>
        /// <param name="data" type="Object">
        ///     A map of data that will be passed to the event handler.
        /// </param>
        /// <param name="fn" type="Function">
        ///     A function to execute each time the event is triggered.
        /// </param>
        /// <returns type="jQuery" />

        if (fn == null) {
            fn = data;
            data = null;
        }

        return arguments.length > 0 ?
            this.on(name, null, data, fn) :
            this.trigger(name);
    };
    jQuery.prototype.unwrap = function () {
        /// <summary>
        ///     Remove the parents of the set of matched elements from the DOM, leaving the matched elements in their place.
        /// </summary>
        /// <returns type="jQuery" />

        return this.parent().each(function () {
            if (!jQuery.nodeName(this, "body")) {
                jQuery(this).replaceWith(this.childNodes);
            }
        }).end();
    };
    jQuery.prototype.val = function (value) {
        /// <summary>
        ///     1: Get the current value of the first element in the set of matched elements.
        ///     &#10;    1.1 - val()
        ///     &#10;2: Set the value of each element in the set of matched elements.
        ///     &#10;    2.1 - val(value) 
        ///     &#10;    2.2 - val(function(index, value))
        /// </summary>
        /// <param name="value" type="String">
        ///     A string of text or an array of strings corresponding to the value of each matched element to set as selected/checked.
        /// </param>
        /// <returns type="jQuery" />

        var hooks, ret, isFunction,
            elem = this[0];

        if (!arguments.length) {
            if (elem) {
                hooks = jQuery.valHooks[elem.type] || jQuery.valHooks[elem.nodeName.toLowerCase()];

                if (hooks && "get" in hooks && (ret = hooks.get(elem, "value")) !== undefined) {
                    return ret;
                }

                ret = elem.value;

                return typeof ret === "string" ?
                    // handle most common string cases
                    ret.replace(rreturn, "") :
                    // handle cases where value is null/undef or number
                    ret == null ? "" : ret;
            }

            return;
        }

        isFunction = jQuery.isFunction(value);

        return this.each(function (i) {
            var val,
                self = jQuery(this);

            if (this.nodeType !== 1) {
                return;
            }

            if (isFunction) {
                val = value.call(this, i, self.val());
            } else {
                val = value;
            }

            // Treat null/undefined as ""; convert numbers to string
            if (val == null) {
                val = "";
            } else if (typeof val === "number") {
                val += "";
            } else if (jQuery.isArray(val)) {
                val = jQuery.map(val, function (value) {
                    return value == null ? "" : value + "";
                });
            }

            hooks = jQuery.valHooks[this.type] || jQuery.valHooks[this.nodeName.toLowerCase()];

            // If set returns undefined, fall back to normal setting
            if (!hooks || !("set" in hooks) || hooks.set(this, val, "value") === undefined) {
                this.value = val;
            }
        });
    };
    jQuery.prototype.width = function (margin, value) {
        /// <summary>
        ///     1: Get the current computed width for the first element in the set of matched elements.
        ///     &#10;    1.1 - width()
        ///     &#10;2: Set the CSS width of each element in the set of matched elements.
        ///     &#10;    2.1 - width(value) 
        ///     &#10;    2.2 - width(function(index, width))
        /// </summary>
        /// <param name="margin" type="Number">
        ///     An integer representing the number of pixels, or an integer along with an optional unit of measure appended (as a string).
        /// </param>
        /// <returns type="jQuery" />

        var chainable = arguments.length && (defaultExtra || typeof margin !== "boolean"),
            extra = defaultExtra || (margin === true || value === true ? "margin" : "border");

        return jQuery.access(this, function (elem, type, value) {
            var doc;

            if (jQuery.isWindow(elem)) {
                // As of 5/8/2012 this will yield incorrect results for Mobile Safari, but there
                // isn't a whole lot we can do. See pull request at this URL for discussion:
                // https://github.com/jquery/jquery/pull/764
                return elem.document.documentElement["client" + name];
            }

            // Get document width or height
            if (elem.nodeType === 9) {
                doc = elem.documentElement;

                // Either scroll[Width/Height] or offset[Width/Height] or client[Width/Height], whichever is greatest
                // unfortunately, this causes bug #3838 in IE6/8 only, but there is currently no good, small way to fix it.
                return Math.max(
                    elem.body["scroll" + name], doc["scroll" + name],
                    elem.body["offset" + name], doc["offset" + name],
                    doc["client" + name]
                );
            }

            return value === undefined ?
                // Get width or height on the element, requesting but not forcing parseFloat
                jQuery.css(elem, type, value, extra) :

                // Set width or height on the element
                jQuery.style(elem, type, value, extra);
        }, type, chainable ? margin : undefined, chainable, null);
    };
    jQuery.prototype.wrap = function (html) {
        /// <summary>
        ///     Wrap an HTML structure around each element in the set of matched elements.
        ///     &#10;1 - wrap(wrappingElement) 
        ///     &#10;2 - wrap(function(index))
        /// </summary>
        /// <param name="html" type="jQuery">
        ///     An HTML snippet, selector expression, jQuery object, or DOM element specifying the structure to wrap around the matched elements.
        /// </param>
        /// <returns type="jQuery" />

        var isFunction = jQuery.isFunction(html);

        return this.each(function (i) {
            jQuery(this).wrapAll(isFunction ? html.call(this, i) : html);
        });
    };
    jQuery.prototype.wrapAll = function (html) {
        /// <summary>
        ///     Wrap an HTML structure around all elements in the set of matched elements.
        /// </summary>
        /// <param name="html" type="jQuery">
        ///     An HTML snippet, selector expression, jQuery object, or DOM element specifying the structure to wrap around the matched elements.
        /// </param>
        /// <returns type="jQuery" />

        if (jQuery.isFunction(html)) {
            return this.each(function (i) {
                jQuery(this).wrapAll(html.call(this, i));
            });
        }

        if (this[0]) {
            // The elements to wrap the target around
            var wrap = jQuery(html, this[0].ownerDocument).eq(0).clone(true);

            if (this[0].parentNode) {
                wrap.insertBefore(this[0]);
            }

            wrap.map(function () {
                var elem = this;

                while (elem.firstChild && elem.firstChild.nodeType === 1) {
                    elem = elem.firstChild;
                }

                return elem;
            }).append(this);
        }

        return this;
    };
    jQuery.prototype.wrapInner = function (html) {
        /// <summary>
        ///     Wrap an HTML structure around the content of each element in the set of matched elements.
        ///     &#10;1 - wrapInner(wrappingElement) 
        ///     &#10;2 - wrapInner(function(index))
        /// </summary>
        /// <param name="html" type="String">
        ///     An HTML snippet, selector expression, jQuery object, or DOM element specifying the structure to wrap around the content of the matched elements.
        /// </param>
        /// <returns type="jQuery" />

        if (jQuery.isFunction(html)) {
            return this.each(function (i) {
                jQuery(this).wrapInner(html.call(this, i));
            });
        }

        return this.each(function () {
            var self = jQuery(this),
                contents = self.contents();

            if (contents.length) {
                contents.wrapAll(html);

            } else {
                self.append(html);
            }
        });
    };
    jQuery.fn = jQuery.prototype;
    jQuery.fn.init.prototype = jQuery.fn;
    window.jQuery = window.$ = jQuery;
})(window);