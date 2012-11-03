/* jquery.signalR.core.js */
/*global window:false */
/*!
 * SignalR JavaScript Library v0.5.3
 * http://signalr.net/
 *
 * Copyright David Fowler and Damian Edwards 2012
 * Licensed under the MIT.
 * https://github.com/SignalR/SignalR/blob/master/LICENSE.md
 */

/// <reference path="Scripts/jquery-1.6.2.js" />
(function ($, window) {
    "use strict";
    
    if (typeof ($) !== "function") {
        // no jQuery!
        throw "SignalR: jQuery not found. Please ensure jQuery is referenced before the SignalR.js file.";
    }
    
    if (!window.JSON) {
        // no JSON!
        throw "SignalR: No JSON parser found. Please ensure json2.js is referenced before the SignalR.js file if you need to support clients without native JSON parsing support, e.g. IE<8.";
    }
    
    var signalR,
        _connection,
        _pageLoaded = false,
        _pageWindow = $(window),

        events = {
            onStart: "onStart",
            onStarting: "onStarting",
            onSending: "onSending",
            onReceived: "onReceived",
            onError: "onError",
            onReconnect: "onReconnect",
            onStateChanged: "onStateChanged",
            onDisconnect: "onDisconnect"
        },

        log = function (msg, logging) {
            if (logging === false) {
                return;
            }
            var m;
            if (typeof (window.console) === "undefined") {
                return;
            }
            m = "[" + new Date().toTimeString() + "] SignalR: " + msg;
            if (window.console.debug) {
                window.console.debug(m);
            } else if (window.console.log) {
                window.console.log(m);
            }
        },

        isCrossDomain = function (url) {
            var link;

            url = $.trim(url);
            if (url.indexOf("http") !== 0) {
                return false;
            }

            // Create an anchor tag.
            link = window.document.createElement("a");
            link.href = url;

            return link.protocol + link.host !== window.location.protocol + window.location.host;
        },

        changeState = function (connection, expectedState, newState) {
            if (expectedState === connection.state) {
                // REVIEW: Should event fire before or after the state change actually occurs?
                $(connection).trigger(events.onStateChanged, [{ oldState: connection.state, newState: newState }]);
                connection.state = newState;
                return true;
            }

            return false;
        },

        isDisconnecting = function (connection) {
            return connection.state === signalR.connectionState.disconnected;
        };

    signalR = function (url, qs, logging) {
        /// <summary>Creates a new SignalR connection for the given url</summary>
        /// <param name="url" type="String">The URL of the long polling endpoint</param>
        /// <param name="qs" type="Object">
        ///     [Optional] Custom querystring parameters to add to the connection URL.
        ///     If an object, every non-function member will be added to the querystring.
        ///     If a string, it's added to the QS as specified.
        /// </param>
        /// <param name="logging" type="Boolean">
        ///     [Optional] A flag indicating whether connection logging is enabled to the browser
        ///     console/log. Defaults to false.
        /// </param>

        return new signalR.fn.init(url, qs, logging);
    };
    
    signalR.events = events;

    signalR.changeState = changeState;

    signalR.isDisconnecting = isDisconnecting;

    signalR.connectionState = {
        connecting: 0,
        connected: 1,
        reconnecting: 2,
        disconnected: 4
    };
    
    signalR.hub = {
        start: function () {
            // This will get replaced with the real hub connection start method when hubs is referenced correctly
            throw "SignalR: Error loading hubs. Ensure your hubs reference is correct, e.g. <script src='/signalr/hubs'></script>.";
        }
    };

    _pageWindow.load(function () { _pageLoaded = true; });

    signalR.fn = signalR.prototype = {
        init: function (url, qs, logging) {
            this.url = url;
            this.qs = qs;
            if (typeof (logging) === "boolean") {
                this.logging = logging;
            }
        },

        ajaxDataType: "json",

        logging: false,

        state: signalR.connectionState.disconnected,

        reconnectDelay: 2000,

        start: function (options, callback) {
            /// <summary>Starts the connection</summary>
            /// <param name="options" type="Object">Options map</param>
            /// <param name="callback" type="Function">A callback function to execute when the connection has started</param>
            var connection = this,
                config = {
                    waitForPageLoad: true,
                    transport: "auto",
                    jsonp: false
                },
                initialize,
                deferred = connection.deferral || $.Deferred(),// Check to see if there is a pre-existing deferral that's being built on, if so we want to keep using it
                parser = window.document.createElement("a");            

            if ($.type(options) === "function") {
                // Support calling with single callback parameter
                callback = options;
            } else if ($.type(options) === "object") {
                $.extend(config, options);
                if ($.type(config.callback) === "function") {
                    callback = config.callback;
                }
            }

            // Check to see if start is being called prior to page load
            // If waitForPageLoad is true we then want to re-direct function call to the window load event
            if (!_pageLoaded && config.waitForPageLoad === true) {
                _pageWindow.load(function () {
                    connection.deferral = deferred;
                    connection.start(options, callback);
                });
                return deferred.promise();
            }

            if (changeState(connection,
                            signalR.connectionState.disconnected,
                            signalR.connectionState.connecting) === false) {
                // Already started, just return
                deferred.resolve(connection);
                return deferred.promise();
            }

            // Resolve the full url
            parser.href = connection.url;
            if (!parser.protocol || parser.protocol === ":") {
                connection.protocol = window.document.location.protocol;
                connection.host = window.document.location.host;
                connection.baseUrl = connection.protocol + "//" + connection.host;
            }
            else {
                connection.protocol = parser.protocol;
                connection.host = parser.host;
                connection.baseUrl = parser.protocol + "//" + parser.host;
            }

            // Set the websocket protocol
            connection.wsProtocol = connection.protocol === "https:" ? "wss://" : "ws://";

            if (isCrossDomain(connection.url)) {
                connection.log("Auto detected cross domain url.");

                if (config.transport === "auto") {
                    // If you didn't say you wanted to use jsonp, determine if it's your only choice
                    // i.e. if your browser doesn't supports CORS
                    if (!config.jsonp) {
                        config.jsonp = !$.support.cors;

                        if (config.jsonp) {
                            connection.log("Using jsonp because this browser doesn't support CORS");
                        }
                    }

                    // If we're using jsonp thn just change to longpolling
                    if (config.jsonp === true) {
                        config.transport = "longPolling";
                    }
                    else {
                        // Otherwise try webSockets and longPolling since SSE doesn't support CORS
                        // TODO: Support XDM with foreverFrame
                        config.transport = ["webSockets", "longPolling"];
                    }
                }
            }

            connection.ajaxDataType = config.jsonp ? "jsonp" : "json";

            $(connection).bind(events.onStart, function (e, data) {
                if ($.type(callback) === "function") {
                    callback.call(connection);
                }
                deferred.resolve(connection);
            });

            initialize = function (transports, index) {
                index = index || 0;
                if (index >= transports.length) {
                    if (!connection.transport) {
                        // No transport initialized successfully
                        $(connection).trigger(events.onError, "SignalR: No transport could be initialized successfully. Try specifying a different transport or none at all for auto initialization.");
                        deferred.reject("SignalR: No transport could be initialized successfully. Try specifying a different transport or none at all for auto initialization.");
                        // Stop the connection if it has connected and move it into the disconnected state
                        connection.stop();
                    }
                    return;
                }

                var transportName = transports[index],
                    transport = $.type(transportName) === "object" ? transportName : signalR.transports[transportName];

                if (transportName.indexOf("_") === 0) {
                    // Private member
                    initialize(transports, index + 1);
                    return;
                }

                transport.start(connection, function () { // success
                    connection.transport = transport;

                    changeState(connection,
                                signalR.connectionState.connecting,
                                signalR.connectionState.connected);

                    $(connection).trigger(events.onStart);

                    _pageWindow.unload(function () { // failure
                        connection.stop(false /* async */);
                    });

                }, function () {
                    initialize(transports, index + 1);
                });
            };

            var url = connection.url + "/negotiate";
            connection.log("Negotiating with '" + url + "'.");
            $.ajax({
                url: url,
                global: false,
                cache: false,
                type: "GET",
                data: {},
                dataType: connection.ajaxDataType,
                error: function (error) {
                    $(connection).trigger(events.onError, [error.responseText]);
                    deferred.reject("SignalR: Error during negotiation request: " + error.responseText);
                    // Stop the connection if negotiate failed
                    connection.stop();
                },
                success: function (res) {
                    connection.appRelativeUrl = res.Url;
                    connection.id = res.ConnectionId;
                    connection.webSocketServerUrl = res.WebSocketServerUrl;

                    if (!res.ProtocolVersion || res.ProtocolVersion !== "1.0") {
                        $(connection).trigger(events.onError, "SignalR: Incompatible protocol version.");
                        deferred.reject("SignalR: Incompatible protocol version.");
                        return;
                    }

                    $(connection).trigger(events.onStarting);

                    var transports = [],
                        supportedTransports = [];

                    $.each(signalR.transports, function (key) {
                        if (key === "webSockets" && !res.TryWebSockets) {
                            // Server said don't even try WebSockets, but keep processing the loop
                            return true;
                        }
                        supportedTransports.push(key);
                    });

                    if ($.isArray(config.transport)) {
                        // ordered list provided
                        $.each(config.transport, function () {
                            var transport = this;
                            if ($.type(transport) === "object" || ($.type(transport) === "string" && $.inArray("" + transport, supportedTransports) >= 0)) {
                                transports.push($.type(transport) === "string" ? "" + transport : transport);
                            }
                        });
                    } else if ($.type(config.transport) === "object" ||
                                    $.inArray(config.transport, supportedTransports) >= 0) {
                            // specific transport provided, as object or a named transport, e.g. "longPolling"
                        transports.push(config.transport);
                    } else { // default "auto"
                        transports = supportedTransports;
                    }
                    initialize(transports);
                }
            });

            return deferred.promise();
        },

        starting: function (callback) {
            /// <summary>Adds a callback that will be invoked before the connection is started</summary>
            /// <param name="callback" type="Function">A callback function to execute when the connection is starting</param>
            /// <returns type="signalR" />
            var connection = this,
                $connection = $(connection);

            $connection.bind(events.onStarting, function (e, data) {
                callback.call(connection);
                // Unbind immediately, we don't want to call this callback again
                $connection.unbind(events.onStarting);
            });

            return connection;
        },

        send: function (data) {
            /// <summary>Sends data over the connection</summary>
            /// <param name="data" type="String">The data to send over the connection</param>
            /// <returns type="signalR" />
            var connection = this;

            if (connection.state === signalR.connectionState.disconnected) {
                // Connection hasn't been started yet
                throw "SignalR: Connection must be started before data can be sent. Call .start() before .send()";
            }

            if (connection.state === signalR.connectionState.connecting) {
                // Connection hasn't been started yet
                throw "SignalR: Connection has not been fully initialized. Use .start().done() or .start().fail() to run logic after the connection has started.";
            }

            connection.transport.send(connection, data);
            // REVIEW: Should we return deferred here?
            return connection;
        },

        sending: function (callback) {
            /// <summary>Adds a callback that will be invoked before anything is sent over the connection</summary>
            /// <param name="callback" type="Function">A callback function to execute before each time data is sent on the connection</param>
            /// <returns type="signalR" />
            var connection = this;
            $(connection).bind(events.onSending, function (e, data) {
                callback.call(connection);
            });
            return connection;
        },

        received: function (callback) {
            /// <summary>Adds a callback that will be invoked after anything is received over the connection</summary>
            /// <param name="callback" type="Function">A callback function to execute when any data is received on the connection</param>
            /// <returns type="signalR" />
            var connection = this;
            $(connection).bind(events.onReceived, function (e, data) {
                callback.call(connection, data);
            });
            return connection;
        },

        stateChanged: function (callback) {
            /// <summary>Adds a callback that will be invoked when the connection state changes</summary>
            /// <param name="callback" type="Function">A callback function to execute when the connection state changes</param>
            /// <returns type="signalR" />
            var connection = this;
            $(connection).bind(events.onStateChanged, function (e, data) {
                callback.call(connection, data);
            });
            return connection;
        },

        error: function (callback) {
            /// <summary>Adds a callback that will be invoked after an error occurs with the connection</summary>
            /// <param name="callback" type="Function">A callback function to execute when an error occurs on the connection</param>
            /// <returns type="signalR" />
            var connection = this;
            $(connection).bind(events.onError, function (e, data) {
                callback.call(connection, data);
            });
            return connection;
        },

        disconnected: function (callback) {
            /// <summary>Adds a callback that will be invoked when the client disconnects</summary>
            /// <param name="callback" type="Function">A callback function to execute when the connection is broken</param>
            /// <returns type="signalR" />
            var connection = this;
            $(connection).bind(events.onDisconnect, function (e, data) {
                callback.call(connection);
            });
            return connection;
        },

        reconnected: function (callback) {
            /// <summary>Adds a callback that will be invoked when the underlying transport reconnects</summary>
            /// <param name="callback" type="Function">A callback function to execute when the connection is restored</param>
            /// <returns type="signalR" />
            var connection = this;
            $(connection).bind(events.onReconnect, function (e, data) {
                callback.call(connection);
            });
            return connection;
        },

        stop: function (async) {
            /// <summary>Stops listening</summary>
            /// <returns type="signalR" />
            var connection = this;

            if (connection.state === signalR.connectionState.disconnected) {
                return;
            }

            try {
                if (connection.transport) {
                    connection.transport.abort(connection, async);
                    connection.transport.stop(connection);
                    connection.transport = null;
                }

                // Trigger the disconnect event
                $(connection).trigger(events.onDisconnect);

                delete connection.messageId;
                delete connection.groups;
            }
            finally {
                changeState(connection, connection.state, signalR.connectionState.disconnected);
            }

            return connection;
        },

        log: function (msg) {
            log(msg, this.logging);
        }
    };

    signalR.fn.init.prototype = signalR.fn;

    signalR.noConflict = function () {
        /// <summary>Reinstates the original value of $.connection and returns the signalR object for manual assignment</summary>
        /// <returns type="signalR" />
        if ($.connection === signalR) {
            $.connection = _connection;
        }
        return signalR;
    };

    if ($.connection) {
        _connection = $.connection;
    }

    $.connection = $.signalR = signalR;

}(window.jQuery, window));
/* jquery.signalR.transports.common.js */
/*global window:false */
/// <reference path="jquery.signalR.core.js" />

(function ($, window) {
    "use strict";

    var signalR = $.signalR,
        events = $.signalR.events;

    signalR.transports = {};

    signalR.transports._logic = {
        addQs: function (url, connection) {
            if (!connection.qs) {
                return url;
            }

            if (typeof (connection.qs) === "object") {
                return url + "&" + $.param(connection.qs);
            }

            if (typeof (connection.qs) === "string") {
                return url + "&" + connection.qs;
            }

            return url + "&" + window.escape(connection.qs.toString());
        },

        getUrl: function (connection, transport, reconnecting, appendReconnectUrl) {
            /// <summary>Gets the url for making a GET based connect request</summary>
            var baseUrl = transport === "webSockets" ? "" : connection.baseUrl,
                url = baseUrl + connection.appRelativeUrl,
                qs = "transport=" + transport + "&connectionId=" + window.escape(connection.id);

            if (connection.data) {
                qs += "&connectionData=" + window.escape(connection.data);
            }

            if (!reconnecting) {
                url = url + "/connect";
            } else {
                if (appendReconnectUrl) {
                    url = url + "/reconnect";
                }
                if (connection.messageId) {
                    qs += "&messageId=" + connection.messageId;
                }
                if (connection.groups) {
                    qs += "&groups=" + window.escape(JSON.stringify(connection.groups));
                }
            }
            url += "?" + qs;
            url = this.addQs(url, connection);
            url += "&tid=" + Math.floor(Math.random() * 11);
            return url;
        },

        ajaxSend: function (connection, data) {
            var url = connection.url + "/send" + "?transport=" + connection.transport.name + "&connectionId=" + window.escape(connection.id);
            url = this.addQs(url, connection);
            return $.ajax({
                url: url,
                global: false,
                type: "POST",
                dataType: connection.ajaxDataType,
                data: {
                    data: data
                },
                success: function (result) {
                    if (result) {
                        $(connection).trigger(events.onReceived, [result]);
                    }
                },
                error: function (errData, textStatus) {
                    if (textStatus === "abort" ||
                        (textStatus === "parsererror" && connection.ajaxDataType === "jsonp")) {
                        // The parsererror happens for sends that don't return any data, and hence
                        // don't write the jsonp callback to the response. This is harder to fix on the server
                        // so just hack around it on the client for now.
                        return;
                    }
                    $(connection).trigger(events.onError, [errData]);
                }
            });
        },

        ajaxAbort: function (connection, async) {
            if (typeof (connection.transport) === "undefined") {
                return;
            }

            // Async by default unless explicitly overidden
            async = typeof async === "undefined" ? true : async;

            var url = connection.url + "/abort" + "?transport=" + connection.transport.name + "&connectionId=" + window.escape(connection.id);
            url = this.addQs(url, connection);
            $.ajax({
                url: url,
                async: async,
                timeout: 1000,
                global: false,
                type: "POST",
                dataType: connection.ajaxDataType,
                data: {}
            });

            connection.log("Fired ajax abort async = " + async);
        },

        processMessages: function (connection, data) {
            var $connection = $(connection);

            if (!data) {
                return;
            }

            if (data.Disconnect) {
                connection.log("Disconnect command received from server");

                // Disconnected by the server
                connection.stop();
                return;
            }

            if (data.Messages) {
                $.each(data.Messages, function () {
                    try {
                        $connection.trigger(events.onReceived, [this]);
                    }
                    catch (e) {
                        connection.log("Error raising received " + e);
                        $(connection).trigger(events.onError, [e]);
                    }
                });
            }

            if (data.MessageId) {
                connection.messageId = data.MessageId;
            }

            if (data.TransportData) {
                connection.groups = data.TransportData.Groups;
            }
        },

        foreverFrame: {
            count: 0,
            connections: {}
        }
    };

}(window.jQuery, window));
/* jquery.signalR.transports.webSockets.js */
/*global window:false */
/// <reference path="jquery.signalR.transports.common.js" />

(function ($, window) {
    "use strict";

    var signalR = $.signalR,
        events = $.signalR.events,
        changeState = $.signalR.changeState,
        transportLogic = signalR.transports._logic;

    signalR.transports.webSockets = {
        name: "webSockets",

        send: function (connection, data) {
            connection.socket.send(data);
        },

        start: function (connection, onSuccess, onFailed) {
            var url,
                opened = false,
                that = this,
                reconnecting = !onSuccess,
                $connection = $(connection);

            if (window.MozWebSocket) {
                window.WebSocket = window.MozWebSocket;
            }

            if (!window.WebSocket) {
                onFailed();
                return;
            }

            if (!connection.socket) {
                if (connection.webSocketServerUrl) {
                    url = connection.webSocketServerUrl;
                }
                else {
                    url = connection.wsProtocol + connection.host;
                }

                // Build the url
                $(connection).trigger(events.onSending);

                url += transportLogic.getUrl(connection, this.name, reconnecting);

                connection.log("Connecting to websocket endpoint '" + url + "'");
                connection.socket = new window.WebSocket(url);
                connection.socket.onopen = function () {
                    opened = true;
                    connection.log("Websocket opened");
                    if (onSuccess) {
                        onSuccess();
                    }
                    else {
                        if (changeState(connection,
                                        signalR.connectionState.reconnecting,
                                        signalR.connectionState.connected) === true) {
                            $connection.trigger(events.onReconnect);
                        }
                    }
                };

                connection.socket.onclose = function (event) {
                    if (!opened) {
                        if (onFailed) {
                            onFailed();
                        }
                        else if (reconnecting) {
                            that.reconnect(connection);
                        }
                        return;
                    }
                    else if (typeof event.wasClean !== "undefined" && event.wasClean === false) {
                            // Ideally this would use the websocket.onerror handler (rather than checking wasClean in onclose) but
                            // I found in some circumstances Chrome won't call onerror. This implementation seems to work on all browsers.
                        $(connection).trigger(events.onError, [event.reason]);
                        connection.log("Unclean disconnect from websocket." + event.reason);
                    }
                    else {
                        connection.log("Websocket closed");
                    }

                    that.reconnect(connection);
                };

                connection.socket.onmessage = function (event) {
                    var data = window.JSON.parse(event.data),
                        $connection;
                    if (data) {
                        $connection = $(connection);

                        if (data.Messages) {
                            transportLogic.processMessages(connection, data);
                        } else {
                            $connection.trigger(events.onReceived, [data]);
                        }
                    }
                };
            }
        },

        reconnect: function (connection) {
            var that = this;
            window.setTimeout(function () {
                that.stop(connection);

                if (connection.state === signalR.connectionState.reconnecting ||
                    changeState(connection,
                                signalR.connectionState.connected,
                                signalR.connectionState.reconnecting) === true) {

                    connection.log("Websocket reconnecting");
                    that.start(connection);
                }
            },
            connection.reconnectDelay);
        },

        stop: function (connection) {
            if (connection.socket !== null) {
                connection.log("Closing the Websocket");
                connection.socket.close();
                connection.socket = null;
            }
        },

        abort: function (connection) {
        }
    };

}(window.jQuery, window));
/* jquery.signalR.transports.serverSentEvents.js */
/*global window:false */
/// <reference path="jquery.signalR.transports.common.js" />

(function ($, window) {
    "use strict";

    var signalR = $.signalR,
        events = $.signalR.events,
        changeState = $.signalR.changeState,
        transportLogic = signalR.transports._logic;

    signalR.transports.serverSentEvents = {
        name: "serverSentEvents",

        timeOut: 3000,

        start: function (connection, onSuccess, onFailed) {
            var that = this,
                opened = false,
                $connection = $(connection),
                reconnecting = !onSuccess,
                url,
                connectTimeOut;

            if (connection.eventSource) {
                connection.log("The connection already has an event source. Stopping it.");
                connection.stop();
            }

            if (!window.EventSource) {
                if (onFailed) {
                    connection.log("This browser doesn't support SSE.");
                    onFailed();
                }
                return;
            }

            $connection.trigger(events.onSending);

            url = transportLogic.getUrl(connection, this.name, reconnecting);

            try {
                connection.log("Attempting to connect to SSE endpoint '" + url + "'");
                connection.eventSource = new window.EventSource(url);
            }
            catch (e) {
                connection.log("EventSource failed trying to connect with error " + e.Message);
                if (onFailed) {
                    // The connection failed, call the failed callback
                    onFailed();
                }
                else {
                    $connection.trigger(events.onError, [e]);
                    if (reconnecting) {
                        // If we were reconnecting, rather than doing initial connect, then try reconnect again
                        that.reconnect(connection);
                    }
                }
                return;
            }

            // After connecting, if after the specified timeout there's no response stop the connection
            // and raise on failed
            connectTimeOut = window.setTimeout(function () {
                if (opened === false) {
                    connection.log("EventSource timed out trying to connect");
                    connection.log("EventSource readyState: " + connection.eventSource.readyState);

                    if (!reconnecting) {
                        that.stop(connection);
                    }

                    if (reconnecting) {
                        // If we're reconnecting and the event source is attempting to connect,
                        // don't keep retrying. This causes duplicate connections to spawn.
                        if (connection.eventSource.readyState !== window.EventSource.CONNECTING &&
                            connection.eventSource.readyState !== window.EventSource.OPEN) {
                            // If we were reconnecting, rather than doing initial connect, then try reconnect again
                            that.reconnect(connection);
                        }
                    } else if (onFailed) {
                        onFailed();
                    }
                }
            },
            that.timeOut);

            connection.eventSource.addEventListener("open", function (e) {
                connection.log("EventSource connected");

                if (connectTimeOut) {
                    window.clearTimeout(connectTimeOut);
                }

                if (opened === false) {
                    opened = true;

                    if (onSuccess) {
                        onSuccess();
                    }

                    if (reconnecting) {
                        if (changeState(connection,
                                        signalR.connectionState.reconnecting,
                                        signalR.connectionState.connected) === true) {
                            $connection.trigger(events.onReconnect);
                        }
                    }
                }
            }, false);

            connection.eventSource.addEventListener("message", function (e) {
                // process messages
                if (e.data === "initialized") {
                    return;
                }
                transportLogic.processMessages(connection, window.JSON.parse(e.data));
            }, false);

            connection.eventSource.addEventListener("error", function (e) {
                if (!opened) {
                    if (onFailed) {
                        onFailed();
                    }
                    return;
                }

                connection.log("EventSource readyState: " + connection.eventSource.readyState);

                if (e.eventPhase === window.EventSource.CLOSED) {
                    // We don't use the EventSource's native reconnect function as it
                    // doesn't allow us to change the URL when reconnecting. We need
                    // to change the URL to not include the /connect suffix, and pass
                    // the last message id we received.
                    connection.log("EventSource reconnecting due to the server connection ending");
                    that.reconnect(connection);
                } else {
                    // connection error
                    connection.log("EventSource error");
                    $connection.trigger(events.onError);
                }
            }, false);
        },

        reconnect: function (connection) {
            var that = this;
            window.setTimeout(function () {
                that.stop(connection);

                if (connection.state === signalR.connectionState.reconnecting ||
                    changeState(connection,
                                signalR.connectionState.connected,
                                signalR.connectionState.reconnecting) === true) {
                    connection.log("EventSource reconnecting");
                    that.start(connection);
                }

            }, connection.reconnectDelay);
        },

        send: function (connection, data) {
            transportLogic.ajaxSend(connection, data);
        },

        stop: function (connection) {
            if (connection && connection.eventSource) {
                connection.log("EventSource calling close()");
                connection.eventSource.close();
                connection.eventSource = null;
                delete connection.eventSource;
            }
        },
        abort: function (connection, async) {
            transportLogic.ajaxAbort(connection, async);
        }
    };

}(window.jQuery, window));
/* jquery.signalR.transports.foreverFrame.js */
/*global window:false */
/// <reference path="jquery.signalR.transports.common.js" />

(function ($, window) {
    "use strict";

    var signalR = $.signalR,
        events = $.signalR.events,
        changeState = $.signalR.changeState,
        transportLogic = signalR.transports._logic;

    signalR.transports.foreverFrame = {
        name: "foreverFrame",

        timeOut: 3000,

        start: function (connection, onSuccess, onFailed) {
            var that = this,
                frameId = (transportLogic.foreverFrame.count += 1),
                url,
                connectTimeOut,
                frame = $("<iframe data-signalr-connection-id='" + connection.id + "' style='position:absolute;top:0;left:0;width:0;height:0;visibility:hidden;' src=''></iframe>");

            if (window.EventSource) {
                // If the browser supports SSE, don't use Forever Frame
                if (onFailed) {
                    connection.log("This brower supports SSE, skipping Forever Frame.");
                    onFailed();
                }
                return;
            }

            $(connection).trigger(events.onSending);

            // Build the url
            url = transportLogic.getUrl(connection, this.name);
            url += "&frameId=" + frameId;

            frame.prop("src", url);
            transportLogic.foreverFrame.connections[frameId] = connection;

            connection.log("Binding to iframe's readystatechange event.");
            frame.bind("readystatechange", function () {
                if ($.inArray(this.readyState, ["loaded", "complete"]) >= 0) {
                    connection.log("Forever frame iframe readyState changed to " + this.readyState + ", reconnecting");

                    that.reconnect(connection);
                }
            });

            connection.frame = frame[0];
            connection.frameId = frameId;

            if (onSuccess) {
                connection.onSuccess = onSuccess;
            }

            $("body").append(frame);

            // After connecting, if after the specified timeout there's no response stop the connection
            // and raise on failed
            // REVIEW: Why is connectTimeOut set here and never used again?
            connectTimeOut = window.setTimeout(function () {
                if (connection.onSuccess) {
                    connection.log("Failed to connect using forever frame source, it timed out after " + that.timeOut + "ms.");
                    that.stop(connection);

                    if (onFailed) {
                        onFailed();
                    }
                }
            }, that.timeOut);
        },

        reconnect: function (connection) {
            var that = this;
            window.setTimeout(function () {
                if (!connection.frame) {
                    return;
                }

                if (connection.state === signalR.connectionState.reconnecting ||
                    changeState(connection,
                                signalR.connectionState.connected,
                                signalR.connectionState.reconnecting) === true) {

                    var frame = connection.frame,
                    src = transportLogic.getUrl(connection, that.name, true) + "&frameId=" + connection.frameId;
                    connection.log("Upating iframe src to '" + src + "'.");
                    frame.src = src;
                }

            }, connection.reconnectDelay);
        },

        send: function (connection, data) {
            transportLogic.ajaxSend(connection, data);
        },

        receive: function (connection, data) {
            var cw;
            transportLogic.processMessages(connection, data);
            // Delete the script & div elements
            connection.frameMessageCount = (connection.frameMessageCount || 0) + 1;
            if (connection.frameMessageCount > 50) {
                connection.frameMessageCount = 0;
                cw = connection.frame.contentWindow || connection.frame.contentDocument;
                if (cw && cw.document) {
                    $("body", cw.document).empty();
                }
            }
        },

        stop: function (connection) {
            var cw = null;
            if (connection.frame) {
                if (connection.frame.stop) {
                    connection.frame.stop();
                } else {
                    cw = connection.frame.contentWindow || connection.frame.contentDocument;
                    if (cw.document && cw.document.execCommand) {
                        cw.document.execCommand("Stop");
                    }
                }
                $(connection.frame).remove();
                delete transportLogic.foreverFrame.connections[connection.frameId];
                connection.frame = null;
                connection.frameId = null;
                delete connection.frame;
                delete connection.frameId;
                connection.log("Stopping forever frame");
            }
        },

        abort: function (connection, async) {
            transportLogic.ajaxAbort(connection, async);
        },

        getConnection: function (id) {
            return transportLogic.foreverFrame.connections[id];
        },

        started: function (connection) {
            if (connection.onSuccess) {
                connection.onSuccess();
                connection.onSuccess = null;
                delete connection.onSuccess;
            }
            else {
                if (changeState(connection,
                                signalR.connectionState.reconnecting,
                                signalR.connectionState.connected) === true) {
                    // If there's no onSuccess handler we assume this is a reconnect
                    $(connection).trigger(events.onReconnect);
                }
            }
        }
    };

}(window.jQuery, window));
/* jquery.signalR.transports.longPolling.js */
/*global window:false */
/// <reference path="jquery.signalR.transports.common.js" />

(function ($, window) {
    "use strict";

    var signalR = $.signalR,
        events = $.signalR.events,
        changeState = $.signalR.changeState,
        isDisconnecting = $.signalR.isDisconnecting,
        transportLogic = signalR.transports._logic;

    signalR.transports.longPolling = {
        name: "longPolling",

        reconnectDelay: 3000,

        start: function (connection, onSuccess, onFailed) {
            /// <summary>Starts the long polling connection</summary>
            /// <param name="connection" type="signalR">The SignalR connection to start</param>
            var that = this,
                initialConnectFired = false;
            
            if (connection.pollXhr) {
                connection.log("Polling xhr requests already exists, aborting.");
                connection.stop();
            }
            
            connection.messageId = null;
            
            window.setTimeout(function () {
                (function poll(instance, raiseReconnect) {
                    $(instance).trigger(events.onSending);

                    var messageId = instance.messageId,
                        connect = (messageId === null),
                        reconnecting = !connect,
                        url = transportLogic.getUrl(instance, that.name, reconnecting, raiseReconnect),
                        reconnectTimeOut = null,
                        reconnectFired = false;

                    if (reconnecting === true && raiseReconnect === true) {
                        if (connection.state !== signalR.connectionState.reconnecting &&
                            changeState(connection,
                                        signalR.connectionState.connected,
                                        signalR.connectionState.reconnecting) === false) {
                            return;
                        }
                    }

                    connection.log("Attempting to connect to '" + url + "' using longPolling.");
                    instance.pollXhr = $.ajax({
                        url: url,
                        global: false,
                        cache: false,
                        type: "GET",
                        dataType: connection.ajaxDataType,
                        success: function (data) {
                            var delay = 0,
                                timedOutReceived = false;

                            if (initialConnectFired === false) {
                                onSuccess();
                                initialConnectFired = true;
                            }

                            if (raiseReconnect === true) {
                                // Fire the reconnect event if it hasn't been fired as yet
                                if (reconnectFired === false) {
                                    connection.log("Raising the reconnect event");

                                    if (changeState(connection,
                                                    signalR.connectionState.reconnecting,
                                                    signalR.connectionState.connected) === true) {

                                        $(instance).trigger(events.onReconnect);
                                        reconnectFired = true;
                                    }
                                }
                            }

                            transportLogic.processMessages(instance, data);
                            if (data &&
                                data.TransportData &&
                                $.type(data.TransportData.LongPollDelay) === "number") {
                                delay = data.TransportData.LongPollDelay;
                            }

                            if (data && data.TimedOut) {
                                timedOutReceived = data.TimedOut;
                            }

                            if (data && data.Disconnect) {
                                return;
                            }

                            if (isDisconnecting(instance) === true) {
                                return;
                            }

                            if (delay > 0) {
                                window.setTimeout(function () {
                                    poll(instance, timedOutReceived);
                                }, delay);
                            } else {
                                poll(instance, timedOutReceived);
                            }
                        },

                        error: function (data, textStatus) {
                            if (textStatus === "abort") {
                                connection.log("Aborted xhr requst.");
                                return;
                            }

                            connection.log("An error occurred using longPolling. Status = " + textStatus + ". " + data.responseText);

                            if (reconnectTimeOut) {
                                // If the request failed then we clear the timeout so that the
                                // reconnect event doesn't get fired
                                window.clearTimeout(reconnectTimeOut);
                            }

                            $(instance).trigger(events.onError, [data.responseText]);

                            window.setTimeout(function () {
                                if (isDisconnecting(instance) === false) {
                                    poll(instance, true);
                                }
                            }, connection.reconnectDelay);
                        }
                    });

                    if (raiseReconnect === true) {
                        reconnectTimeOut = window.setTimeout(function () {
                            if (reconnectFired === false) {
                                if (changeState(connection,
                                                signalR.connectionState.reconnecting,
                                                signalR.connectionState.connected) === true) {

                                    $(instance).trigger(events.onReconnect);
                                    reconnectFired = true;
                                }
                            }
                        },
                        that.reconnectDelay);
                    }

                }(connection));

                // Now connected
                // There's no good way know when the long poll has actually started so
                // we assume it only takes around 150ms (max) to start the connection
                window.setTimeout(function () {
                    if (initialConnectFired === false) {
                        onSuccess();
                        initialConnectFired = true;
                    }
                }, 150);

            }, 250); // Have to delay initial poll so Chrome doesn't show loader spinner in tab
        },

        send: function (connection, data) {
            transportLogic.ajaxSend(connection, data);
        },

        stop: function (connection) {
            /// <summary>Stops the long polling connection</summary>
            /// <param name="connection" type="signalR">The SignalR connection to stop</param>
            if (connection.pollXhr) {
                connection.pollXhr.abort();
                connection.pollXhr = null;
                delete connection.pollXhr;
            }
        },

        abort: function (connection, async) {
            transportLogic.ajaxAbort(connection, async);
        }
    };

}(window.jQuery, window));
/* jquery.signalR.hubs.js */
/*global window:false */
/// <reference path="jquery.signalR.core.js" />

(function ($, window) {
    "use strict";

    // we use a global id for tracking callbacks so the server doesn't have to send extra info like hub name
    var callbackId = 0,
        callbacks = {},
        eventNamespace = "proxy.";

    // Array.prototype.map
    if (!Array.prototype.hasOwnProperty("map")) {
        Array.prototype.map = function (fun, thisp) {
            var arr = this,
                i,
                length = arr.length,
                result = [];
            for (i = 0; i < length; i += 1) {
                if (arr.hasOwnProperty(i)) {
                    result[i] = fun.call(thisp, arr[i], i, arr);
                }
            }
            return result;
        };
    }

    function getArgValue(a) {
        return $.isFunction(a) ? null : ($.type(a) === "undefined" ? null : a);
    }

    // hubProxy
    function hubProxy(hubConnection, hubName) {
        /// <summary>
        ///     Creates a new proxy object for the given hub connection that can be used to invoke
        ///     methods on server hubs and handle client method invocation requests from the server.
        /// </summary>
        return new hubProxy.fn.init(hubConnection, hubName);
    }

    hubProxy.fn = hubProxy.prototype = {
        init: function (connection, hubName) {
            this.state = {};
            this.connection = connection;
            this.hubName = hubName;
            this.subscribed = false;
        },

        on: function (eventName, callback) {
            /// <summary>Wires up a callback to be invoked when a invocation request is received from the server hub.</summary>
            /// <param name="eventName" type="String">The name of the hub event to register the callback for.</param>
            /// <param name="callback" type="Function">The callback to be invoked.</param>
            var self = this;

            // Normalize the event name to lowercase
            eventName = eventName.toLowerCase();

            $(self).bind(eventNamespace + eventName, function (e, data) {
                callback.apply(self, data);
            });
            self.subscribed = true;
            return self;
        },

        invoke: function (methodName) {
            /// <summary>Invokes a server hub method with the given arguments.</summary>
            /// <param name="methodName" type="String">The name of the server hub method.</param>

            var self = this,
                args = $.makeArray(arguments).slice(1),
                userCallback = args[args.length - 1], // last argument
                methodArgs = $.type(userCallback) === "function" ? args.slice(0, args.length - 1) /* all but last */ : args,
                argValues = methodArgs.map(getArgValue),
                data = { hub: self.hubName, method: methodName, args: argValues, state: self.state, id: callbackId },
                d = $.Deferred(),
                callback = function (result) {
                    // Update the hub state
                    $.extend(this.state, result.State);

                    if (result.Error) {
                        // Server hub method threw an exception, log it & reject the deferred
                        if (result.StackTrace) {
                            self.connection.log(result.Error + "\n" + result.StackTrace);
                        }
                        d.rejectWith(self, [result.Error]);
                    } else {
                        // Server invocation succeeded, invoke any user callback & resolve the deferred
                        if ($.type(userCallback) === "function") {
                            userCallback.call(self, result.Result);
                        }
                        d.resolveWith(self, [result.Result]);
                    }
                };

            callbacks[callbackId.toString()] = { scope: self, method: callback };
            callbackId += 1;
            self.connection.send(window.JSON.stringify(data));

            return d.promise();
        }
    };

    hubProxy.fn.init.prototype = hubProxy.fn;


    // hubConnection
    function hubConnection(url, options) {
        /// <summary>Creates a new hub connection.</summary>
        /// <param name="url" type="String">[Optional] The hub route url, defaults to "/signalr".</param>
        /// <param name="options" type="Object">[Optional] Settings to use when creating the hubConnection.</param>
        var settings = {
            qs: null,
            logging: false,
            useDefaultPath : true
        };

        $.extend(settings, options);

        if (!url || settings.useDefaultPath) {
            url = (url || "") + "/signalr";
        }
        return new hubConnection.fn.init(url, settings);
    }

    hubConnection.fn = hubConnection.prototype = $.connection();

    hubConnection.fn.init = function (url, options) {
        var settings = {
                qs: null,
                logging: false,
                useDefaultPath: true
            },
            connection = this;

        $.extend(settings, options);

        // Call the base constructor
        $.signalR.fn.init.call(connection, url, settings.qs, settings.logging);

        // Object to store hub proxies for this connection
        connection.proxies = {};

        // Wire up the sending handler
        connection.sending(function () {
            // Set the connection's data object with all the hub proxies with active subscriptions.
            // These proxies will receive notifications from the server.
            var subscribedHubs = [];

            $.each(this.proxies, function (key) {
                if (this.subscribed) {
                    subscribedHubs.push({ name: key });
                }
            });

            this.data = window.JSON.stringify(subscribedHubs);
        });

        // Wire up the received handler
        connection.received(function (data) {
            var proxy, dataCallbackId, callback, hubName, eventName;
            if (!data) {
                return;
            }

            if (typeof (data.Id) !== "undefined") {
                // We received the return value from a server method invocation, look up callback by id and call it
                dataCallbackId = data.Id.toString();
                callback = callbacks[dataCallbackId];
                if (callback) {
                    // Delete the callback from the proxy
                    callbacks[dataCallbackId] = null;
                    delete callbacks[dataCallbackId];

                    // Invoke the callback
                    callback.method.call(callback.scope, data);
                }
            } else {
                // We received a client invocation request, i.e. broadcast from server hub
                connection.log("Triggering client hub event '" + data.Method + "' on hub '" + data.Hub + "'.");

                // Normalize the names to lowercase
                hubName = data.Hub.toLowerCase();
                eventName = data.Method.toLowerCase();
                
                // Trigger the local invocation event
                proxy = this.proxies[hubName];

                // Update the hub state
                $.extend(proxy.state, data.State);
                $(proxy).trigger(eventNamespace + eventName, [data.Args]);
            }
        });
    };

    hubConnection.fn.createProxy = function (hubName) {
        /// <summary>
        ///     Creates a new proxy object for the given hub connection that can be used to invoke
        ///     methods on server hubs and handle client method invocation requests from the server.
        /// </summary>
        /// <param name="hubName" type="String">
        ///     The name of the hub on the server to create the proxy for.
        /// </param>

        // Normalize the name to lowercase
        hubName = hubName.toLowerCase();

        var proxy = this.proxies[hubName];
        if (!proxy) {
            proxy = hubProxy(this, hubName);
            this.proxies[hubName] = proxy;
        }
        return proxy;
    };

    hubConnection.fn.init.prototype = hubConnection.fn;

    $.hubConnection = hubConnection;

} (window.jQuery, window));
