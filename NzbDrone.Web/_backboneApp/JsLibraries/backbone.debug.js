(function () {
    var __bind = function (fn, me) { return function () { return fn.apply(me, arguments); }; };

    window.Backbone.Debug = (function () {

        function Debug() {
            this._hookPrototype = __bind(this._hookPrototype, this);
            this._hookMethod = __bind(this._hookMethod, this);
            this._logSync = __bind(this._logSync, this);
            this._logEvent = __bind(this._logEvent, this);
            this._saveObjects = __bind(this._saveObjects, this);
            this._hookSync = __bind(this._hookSync, this);
            this._hookEvents = __bind(this._hookEvents, this);
            this._trackObjects = __bind(this._trackObjects, this);
            this.off = __bind(this.off, this);
            this.on = __bind(this.on, this);
            this.routers = __bind(this.routers, this);
            this.views = __bind(this.views, this);
            this.models = __bind(this.models, this);
            this.collections = __bind(this.collections, this); this._options = {
                'log:events': true,
                'log:sync': true
            };
            this._objects = {
                Collection: {},
                Model: {},
                View: {},
                Router: {}
            };
            this._trackObjects();
            this._hookEvents();
            this._hookSync();
        }

        Debug.prototype.collections = function () {
            return this._objects.Collection;
        };

        Debug.prototype.models = function () {
            return this._objects.Model;
        };

        Debug.prototype.views = function () {
            return this._objects.View;
        };

        Debug.prototype.routers = function () {
            return this._objects.Router;
        };

        Debug.prototype.on = function (option) {
            if (option != null) {
                return this._options[option] = true;
            } else {
                this._options['log:events'] = true;
                return this._options['log:sync'] = true;
            }
        };

        Debug.prototype.off = function (option) {
            if (option != null) {
                return this._options[option] = false;
            } else {
                this._options['log:events'] = false;
                return this._options['log:sync'] = false;
            }
        };

        Debug.prototype._trackObjects = function () {
            this._hookPrototype('Collection', 'constructor', this._saveObjects);
            this._hookPrototype('Model', 'constructor', this._saveObjects);
            this._hookPrototype('View', 'constructor', this._saveObjects);
            return this._hookPrototype('Router', 'constructor', this._saveObjects);
        };

        Debug.prototype._hookEvents = function () {
            this._hookPrototype('Collection', 'trigger', this._logEvent);
            this._hookPrototype('Model', 'trigger', this._logEvent);
            this._hookPrototype('View', 'trigger', this._logEvent);
            return this._hookPrototype('Router', 'trigger', this._logEvent);
        };

        Debug.prototype._hookSync = function () {
            return this._hookMethod('sync', this._logSync);
        };

        Debug.prototype._saveObjects = function (type, method, object) {
            return this._objects[type][object.constructor.name + ':' + object.cid] = object;
        };

        Debug.prototype._logEvent = function (parent_object, method, object, args) {
            if (this._options['log:events']) {
                return console.log("" + args[0] + " - ", object);
            }
        };

        Debug.prototype._logSync = function (method, object, args) {
            if (this._options['log:sync'] === true) {
                return console.log("sync - " + args[0], args[1]);
            }
        };

        Debug.prototype._hookMethod = function (method, action) {
            var original;
            original = window.Backbone[method];
            return window.Backbone[method] = function () {
                var ret;
                ret = original.apply(this, arguments);
                action(method, this, arguments);
                return ret;
            };
        };

        Debug.prototype._hookPrototype = function (object, method, action) {
            var original;
            original = window.Backbone[object].prototype[method];
            return window.Backbone[object].prototype[method] = function () {
                var ret;
                ret = original.apply(this, arguments);
                action(object, method, this, arguments);
                return ret;
            };
        };

        return Debug;

    })();

    window.Backbone.debug = new Backbone.Debug();

}).call(this);