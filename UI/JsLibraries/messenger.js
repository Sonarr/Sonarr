/*! messenger 1.3.3 */
/*
 * This file begins the output concatenated into messenger.js
 *
 * It establishes the Messenger object while preserving whatever it was before
 * (for noConflict), and making it a callable function.
 */

(function(){
    var _prevMessenger = window.Messenger;
    var localMessenger;

    localMessenger = window.Messenger = function(){
        return localMessenger._call.apply(this, arguments);
    }

    window.Messenger.noConflict = function(){
        window.Messenger = _prevMessenger;

        return localMessenger;
    }
})();

/*
 * This file contains shims for when Underscore and Backbone
 * are not included.
 *
 * Portions taken from Underscore.js and Backbone.js
 * Both of which are Copyright (c) 2009-2013 Jeremy Ashkenas, DocumentCloud
 */
window.Messenger._ = (function() {
    if (window._)
        return window._

    var ArrayProto = Array.prototype, ObjProto = Object.prototype, FuncProto = Function.prototype;

    // Create quick reference variables for speed access to core prototypes.
    var push             = ArrayProto.push,
            slice            = ArrayProto.slice,
            concat           = ArrayProto.concat,
            toString         = ObjProto.toString,
            hasOwnProperty   = ObjProto.hasOwnProperty;

    // All **ECMAScript 5** native function implementations that we hope to use
    // are declared here.
    var
        nativeForEach      = ArrayProto.forEach,
        nativeMap          = ArrayProto.map,
        nativeReduce       = ArrayProto.reduce,
        nativeReduceRight  = ArrayProto.reduceRight,
        nativeFilter       = ArrayProto.filter,
        nativeEvery        = ArrayProto.every,
        nativeSome         = ArrayProto.some,
        nativeIndexOf      = ArrayProto.indexOf,
        nativeLastIndexOf  = ArrayProto.lastIndexOf,
        nativeIsArray      = Array.isArray,
        nativeKeys         = Object.keys,
        nativeBind         = FuncProto.bind;

    // Create a safe reference to the Underscore object for use below.
    var _ = {};

    // Establish the object that gets returned to break out of a loop iteration.
    var breaker = {};
  
    var each = _.each = _.forEach = function(obj, iterator, context) {
        if (obj == null) return;
        if (nativeForEach && obj.forEach === nativeForEach) {
            obj.forEach(iterator, context);
        } else if (obj.length === +obj.length) {
            for (var i = 0, l = obj.length; i < l; i++) {
                if (iterator.call(context, obj[i], i, obj) === breaker) return;
            }
        } else {
            for (var key in obj) {
                if (_.has(obj, key)) {
                    if (iterator.call(context, obj[key], key, obj) === breaker) return;
                }
            }
        }
    };

    _.result = function(object, property) {
        if (object == null) return null;
        var value = object[property];
        return _.isFunction(value) ? value.call(object) : value;
    };

    _.once = function(func) {
        var ran = false, memo;
        return function() {
            if (ran) return memo;
            ran = true;
            memo = func.apply(this, arguments);
            func = null;
            return memo;
        };
    };

    var idCounter = 0;
    _.uniqueId = function(prefix) {
        var id = ++idCounter + '';
        return prefix ? prefix + id : id;
    };

    _.filter = _.select = function(obj, iterator, context) {
        var results = [];
        if (obj == null) return results;
        if (nativeFilter && obj.filter === nativeFilter) return obj.filter(iterator, context);
        each(obj, function(value, index, list) {
            if (iterator.call(context, value, index, list)) results[results.length] = value;
        });
        return results;
    };

    // Add some isType methods: isArguments, isFunction, isString, isNumber, isDate, isRegExp.
    each(['Arguments', 'Function', 'String', 'Number', 'Date', 'RegExp'], function(name) {
        _['is' + name] = function(obj) {
            return toString.call(obj) == '[object ' + name + ']';
        };
    });

    _.defaults = function(obj) {
        each(slice.call(arguments, 1), function(source) {
            if (source) {
                for (var prop in source) {
                    if (obj[prop] == null) obj[prop] = source[prop];
                }
            }
        });
        return obj;
    };

    _.extend = function(obj) {
        each(slice.call(arguments, 1), function(source) {
            if (source) {
                for (var prop in source) {
                    obj[prop] = source[prop];
                }
            }
        });
        return obj;
    };

    _.keys = nativeKeys || function(obj) {
        if (obj !== Object(obj)) throw new TypeError('Invalid object');
        var keys = [];
        for (var key in obj) if (_.has(obj, key)) keys[keys.length] = key;
        return keys;
    };

    _.bind = function(func, context) {
        if (func.bind === nativeBind && nativeBind) return nativeBind.apply(func, slice.call(arguments, 1));
        var args = slice.call(arguments, 2);
        return function() {
            return func.apply(context, args.concat(slice.call(arguments)));
        };
    };

    _.isObject = function(obj) {
        return obj === Object(obj);
    };

    return _;
})();

window.Messenger.Events = (function() {
    if (window.Backbone && Backbone.Events) {
        return Backbone.Events;
    }

    var eventsShim = function() {
        var eventSplitter = /\s+/;

        var eventsApi = function(obj, action, name, rest) {
            if (!name) return true;
            if (typeof name === 'object') {
                for (var key in name) {
                    obj[action].apply(obj, [key, name[key]].concat(rest));
                }
            } else if (eventSplitter.test(name)) {
                var names = name.split(eventSplitter);
                for (var i = 0, l = names.length; i < l; i++) {
                    obj[action].apply(obj, [names[i]].concat(rest));
                }
            } else {
                return true;
            }
        };

        var triggerEvents = function(events, args) {
            var ev, i = -1, l = events.length;
            switch (args.length) {
            case 0: while (++i < l) (ev = events[i]).callback.call(ev.ctx);
            return;
            case 1: while (++i < l) (ev = events[i]).callback.call(ev.ctx, args[0]);
            return;
            case 2: while (++i < l) (ev = events[i]).callback.call(ev.ctx, args[0], args[1]);
            return;
            case 3: while (++i < l) (ev = events[i]).callback.call(ev.ctx, args[0], args[1], args[2]);
            return;
            default: while (++i < l) (ev = events[i]).callback.apply(ev.ctx, args);
            }
        };

        var Events = {

            on: function(name, callback, context) {
                if (!(eventsApi(this, 'on', name, [callback, context]) && callback)) return this;
                this._events || (this._events = {});
                var list = this._events[name] || (this._events[name] = []);
                list.push({callback: callback, context: context, ctx: context || this});
                return this;
            },

            once: function(name, callback, context) {
                if (!(eventsApi(this, 'once', name, [callback, context]) && callback)) return this;
                var self = this;
                var once = _.once(function() {
                    self.off(name, once);
                    callback.apply(this, arguments);
                });
                once._callback = callback;
                this.on(name, once, context);
                return this;
            },

            off: function(name, callback, context) {
                var list, ev, events, names, i, l, j, k;
                if (!this._events || !eventsApi(this, 'off', name, [callback, context])) return this;
                if (!name && !callback && !context) {
                    this._events = {};
                    return this;
                }

                names = name ? [name] : _.keys(this._events);
                for (i = 0, l = names.length; i < l; i++) {
                    name = names[i];
                    if (list = this._events[name]) {
                        events = [];
                        if (callback || context) {
                            for (j = 0, k = list.length; j < k; j++) {
                                ev = list[j];
                                if ((callback && callback !== ev.callback &&
                                                                 callback !== ev.callback._callback) ||
                                        (context && context !== ev.context)) {
                                    events.push(ev);
                                }
                            }
                        }
                        this._events[name] = events;
                    }
                }

                return this;
            },

            trigger: function(name) {
                if (!this._events) return this;
                var args = Array.prototype.slice.call(arguments, 1);
                if (!eventsApi(this, 'trigger', name, args)) return this;
                var events = this._events[name];
                var allEvents = this._events.all;
                if (events) triggerEvents(events, args);
                if (allEvents) triggerEvents(allEvents, arguments);
                return this;
            },

            listenTo: function(obj, name, callback) {
                var listeners = this._listeners || (this._listeners = {});
                var id = obj._listenerId || (obj._listenerId = _.uniqueId('l'));
                listeners[id] = obj;
                obj.on(name, typeof name === 'object' ? this : callback, this);
                return this;
            },

            stopListening: function(obj, name, callback) {
                var listeners = this._listeners;
                if (!listeners) return;
                if (obj) {
                    obj.off(name, typeof name === 'object' ? this : callback, this);
                    if (!name && !callback) delete listeners[obj._listenerId];
                } else {
                    if (typeof name === 'object') callback = this;
                    for (var id in listeners) {
                        listeners[id].off(name, callback, this);
                    }
                    this._listeners = {};
                }
                return this;
            }
        };

        Events.bind   = Events.on;
        Events.unbind = Events.off;
        return Events;
    };
    return eventsShim();
})();

(function() {
  var $, ActionMessenger, BaseView, Events, RetryingMessage, _, _Message, _Messenger, _ref, _ref1, _ref2,
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __slice = [].slice,
    __indexOf = [].indexOf || function(item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

  $ = jQuery;

  _ = (_ref = window._) != null ? _ref : window.Messenger._;

  Events = (_ref1 = typeof Backbone !== "undefined" && Backbone !== null ? Backbone.Events : void 0) != null ? _ref1 : window.Messenger.Events;

  BaseView = (function() {

    function BaseView(options) {
      $.extend(this, Events);
      if (_.isObject(options)) {
        if (options.el) {
          this.setElement(options.el);
        }
        this.model = options.model;
      }
      this.initialize.apply(this, arguments);
    }

    BaseView.prototype.setElement = function(el) {
      this.$el = $(el);
      return this.el = this.$el[0];
    };

    BaseView.prototype.delegateEvents = function(events) {
      var delegateEventSplitter, eventName, key, match, method, selector, _results;
      if (!(events || (events = _.result(this, 'events')))) {
        return;
      }
      delegateEventSplitter = /^(\S+)\s*(.*)$/;
      this.undelegateEvents();
      _results = [];
      for (key in events) {
        method = events[key];
        if (!_.isFunction(method)) {
          method = this[events[key]];
        }
        if (!method) {
          throw new Error("Method " + events[key] + " does not exist");
        }
        match = key.match(delegateEventSplitter);
        eventName = match[1];
        selector = match[2];
        method = _.bind(method, this);
        eventName += ".delegateEvents" + this.cid;
        if (selector === '') {
          _results.push(this.$el.on(eventName, method));
        } else {
          _results.push(this.$el.on(eventName, selector, method));
        }
      }
      return _results;
    };

    BaseView.prototype.undelegateEvents = function() {
      return this.$el.off(".delegateEvents" + this.cid);
    };

    BaseView.prototype.remove = function() {
      this.undelegateEvents();
      return this.$el.remove();
    };

    return BaseView;

  })();

  _Message = (function(_super) {

    __extends(_Message, _super);

    function _Message() {
      return _Message.__super__.constructor.apply(this, arguments);
    }

    _Message.prototype.defaults = {
      hideAfter: 10,
      scroll: true
    };

    _Message.prototype.initialize = function(opts) {
      if (opts == null) {
        opts = {};
      }
      this.shown = false;
      this.rendered = false;
      this.messenger = opts.messenger;
      return this.options = $.extend({}, this.options, opts, this.defaults);
    };

    _Message.prototype.show = function() {
      var wasShown;
      if (!this.rendered) {
        this.render();
      }
      this.$message.removeClass('messenger-hidden');
      wasShown = this.shown;
      this.shown = true;
      if (!wasShown) {
        return this.trigger('show');
      }
    };

    _Message.prototype.hide = function() {
      var wasShown;
      if (!this.rendered) {
        return;
      }
      this.$message.addClass('messenger-hidden');
      wasShown = this.shown;
      this.shown = false;
      if (wasShown) {
        return this.trigger('hide');
      }
    };

    _Message.prototype.cancel = function() {
      return this.hide();
    };

    _Message.prototype.update = function(opts) {
      var _ref2,
        _this = this;
      if (_.isString(opts)) {
        opts = {
          message: opts
        };
      }
      $.extend(this.options, opts);
      this.lastUpdate = new Date();
      this.rendered = false;
      this.events = (_ref2 = this.options.events) != null ? _ref2 : {};
      this.render();
      this.actionsToEvents();
      this.delegateEvents();
      this.checkClickable();
      if (this.options.hideAfter) {
        this.$message.addClass('messenger-will-hide-after');
        if (this._hideTimeout != null) {
          clearTimeout(this._hideTimeout);
        }
        this._hideTimeout = setTimeout(function() {
          return _this.hide();
        }, this.options.hideAfter * 1000);
      } else {
        this.$message.removeClass('messenger-will-hide-after');
      }
      if (this.options.hideOnNavigate) {
        this.$message.addClass('messenger-will-hide-on-navigate');
        if ((typeof Backbone !== "undefined" && Backbone !== null ? Backbone.history : void 0) != null) {
          Backbone.history.on('route', function() {
            return _this.hide();
          });
        }
      } else {
        this.$message.removeClass('messenger-will-hide-on-navigate');
      }
      return this.trigger('update', this);
    };

    _Message.prototype.scrollTo = function() {
      if (!this.options.scroll) {
        return;
      }
      return $.scrollTo(this.$el, {
        duration: 400,
        offset: {
          left: 0,
          top: -20
        }
      });
    };

    _Message.prototype.timeSinceUpdate = function() {
      if (this.lastUpdate) {
        return (new Date) - this.lastUpdate;
      } else {
        return null;
      }
    };

    _Message.prototype.actionsToEvents = function() {
      var act, name, _ref2, _results,
        _this = this;
      _ref2 = this.options.actions;
      _results = [];
      for (name in _ref2) {
        act = _ref2[name];
        _results.push(this.events["click [data-action=\"" + name + "\"] a"] = (function(act) {
          return function(e) {
            e.preventDefault();
            e.stopPropagation();
            _this.trigger("action:" + name, act, e);
            return act.action.call(_this, e, _this);
          };
        })(act));
      }
      return _results;
    };

    _Message.prototype.checkClickable = function() {
      var evt, name, _ref2, _results;
      _ref2 = this.events;
      _results = [];
      for (name in _ref2) {
        evt = _ref2[name];
        if (name === 'click') {
          _results.push(this.$message.addClass('messenger-clickable'));
        } else {
          _results.push(void 0);
        }
      }
      return _results;
    };

    _Message.prototype.undelegateEvents = function() {
      var _ref2;
      _Message.__super__.undelegateEvents.apply(this, arguments);
      return (_ref2 = this.$message) != null ? _ref2.removeClass('messenger-clickable') : void 0;
    };

    _Message.prototype.parseActions = function() {
      var act, actions, n_act, name, _ref2, _ref3;
      actions = [];
      _ref2 = this.options.actions;
      for (name in _ref2) {
        act = _ref2[name];
        n_act = $.extend({}, act);
        n_act.name = name;
        if ((_ref3 = n_act.label) == null) {
          n_act.label = name;
        }
        actions.push(n_act);
      }
      return actions;
    };

    _Message.prototype.template = function(opts) {
      var $action, $actions, $cancel, $link, $message, $text, action, _i, _len, _ref2,
        _this = this;
      $message = $("<div class='messenger-message message alert " + opts.type + " message-" + opts.type + " alert-" + opts.type + "'>");
      if (opts.showCloseButton) {
        $cancel = $('<button type="button" class="close" data-dismiss="alert">&times;</button>');
        $cancel.click(function() {
          _this.cancel();
          return true;
        });
        $message.append($cancel);
      }
      $text = $("<div class=\"messenger-message-inner\">" + opts.message + "</div>");
      $message.append($text);
      if (opts.actions.length) {
        $actions = $('<div class="messenger-actions">');
      }
      _ref2 = opts.actions;
      for (_i = 0, _len = _ref2.length; _i < _len; _i++) {
        action = _ref2[_i];
        $action = $('<span>');
        $action.attr('data-action', "" + action.name);
        $link = $('<a>');
        $link.html(action.label);
        $action.append($('<span class="messenger-phrase">'));
        $action.append($link);
        $actions.append($action);
      }
      $message.append($actions);
      return $message;
    };

    _Message.prototype.render = function() {
      var opts;
      if (this.rendered) {
        return;
      }
      if (!this._hasSlot) {
        this.setElement(this.messenger._reserveMessageSlot(this));
        this._hasSlot = true;
      }
      opts = $.extend({}, this.options, {
        actions: this.parseActions()
      });
      this.$message = $(this.template(opts));
      this.$el.html(this.$message);
      this.shown = true;
      this.rendered = true;
      return this.trigger('render');
    };

    return _Message;

  })(BaseView);

  RetryingMessage = (function(_super) {

    __extends(RetryingMessage, _super);

    function RetryingMessage() {
      return RetryingMessage.__super__.constructor.apply(this, arguments);
    }

    RetryingMessage.prototype.initialize = function() {
      RetryingMessage.__super__.initialize.apply(this, arguments);
      return this._timers = {};
    };

    RetryingMessage.prototype.cancel = function() {
      this.clearTimers();
      this.hide();
      if ((this._actionInstance != null) && (this._actionInstance.abort != null)) {
        return this._actionInstance.abort();
      }
    };

    RetryingMessage.prototype.clearTimers = function() {
      var name, timer, _ref2, _ref3;
      _ref2 = this._timers;
      for (name in _ref2) {
        timer = _ref2[name];
        clearTimeout(timer);
      }
      this._timers = {};
      return (_ref3 = this.$message) != null ? _ref3.removeClass('messenger-retry-soon messenger-retry-later') : void 0;
    };

    RetryingMessage.prototype.render = function() {
      var action, name, _ref2, _results;
      RetryingMessage.__super__.render.apply(this, arguments);
      this.clearTimers();
      _ref2 = this.options.actions;
      _results = [];
      for (name in _ref2) {
        action = _ref2[name];
        if (action.auto) {
          _results.push(this.startCountdown(name, action));
        } else {
          _results.push(void 0);
        }
      }
      return _results;
    };

    RetryingMessage.prototype.renderPhrase = function(action, time) {
      var phrase;
      phrase = action.phrase.replace('TIME', this.formatTime(time));
      return phrase;
    };

    RetryingMessage.prototype.formatTime = function(time) {
      var pluralize;
      pluralize = function(num, str) {
        num = Math.floor(num);
        if (num !== 1) {
          str = str + 's';
        }
        return 'in ' + num + ' ' + str;
      };
      if (Math.floor(time) === 0) {
        return 'now...';
      }
      if (time < 60) {
        return pluralize(time, 'second');
      }
      time /= 60;
      if (time < 60) {
        return pluralize(time, 'minute');
      }
      time /= 60;
      return pluralize(time, 'hour');
    };

    RetryingMessage.prototype.startCountdown = function(name, action) {
      var $phrase, remaining, tick, _ref2,
        _this = this;
      if (this._timers[name] != null) {
        return;
      }
      $phrase = this.$message.find("[data-action='" + name + "'] .messenger-phrase");
      remaining = (_ref2 = action.delay) != null ? _ref2 : 3;
      if (remaining <= 10) {
        this.$message.removeClass('messenger-retry-later');
        this.$message.addClass('messenger-retry-soon');
      } else {
        this.$message.removeClass('messenger-retry-soon');
        this.$message.addClass('messenger-retry-later');
      }
      tick = function() {
        var delta;
        $phrase.text(_this.renderPhrase(action, remaining));
        if (remaining > 0) {
          delta = Math.min(remaining, 1);
          remaining -= delta;
          return _this._timers[name] = setTimeout(tick, delta * 1000);
        } else {
          _this.$message.removeClass('messenger-retry-soon messenger-retry-later');
          delete _this._timers[name];
          return action.action();
        }
      };
      return tick();
    };

    return RetryingMessage;

  })(_Message);

  _Messenger = (function(_super) {

    __extends(_Messenger, _super);

    function _Messenger() {
      return _Messenger.__super__.constructor.apply(this, arguments);
    }

    _Messenger.prototype.tagName = 'ul';

    _Messenger.prototype.className = 'messenger';

    _Messenger.prototype.messageDefaults = {
      type: 'info'
    };

    _Messenger.prototype.initialize = function(options) {
      this.options = options != null ? options : {};
      this.history = [];
      return this.messageDefaults = $.extend({}, this.messageDefaults, this.options.messageDefaults);
    };

    _Messenger.prototype.render = function() {
      return this.updateMessageSlotClasses();
    };

    _Messenger.prototype.findById = function(id) {
      return _.filter(this.history, function(rec) {
        return rec.msg.options.id === id;
      });
    };

    _Messenger.prototype._reserveMessageSlot = function(msg) {
      var $slot, dmsg,
        _this = this;
      $slot = $('<li>');
      $slot.addClass('messenger-message-slot');
      this.$el.prepend($slot);
      this.history.push({
        msg: msg,
        $slot: $slot
      });
      this._enforceIdConstraint(msg);
      msg.on('update', function() {
        return _this._enforceIdConstraint(msg);
      });
      while (this.options.maxMessages && this.history.length > this.options.maxMessages) {
        dmsg = this.history.shift();
        dmsg.msg.remove();
        dmsg.$slot.remove();
      }
      return $slot;
    };

    _Messenger.prototype._enforceIdConstraint = function(msg) {
      var entry, _i, _len, _msg, _ref2;
      if (msg.options.id == null) {
        return;
      }
      _ref2 = this.history;
      for (_i = 0, _len = _ref2.length; _i < _len; _i++) {
        entry = _ref2[_i];
        _msg = entry.msg;
        if ((_msg.options.id != null) && _msg.options.id === msg.options.id && msg !== _msg) {
          if (msg.options.singleton) {
            msg.hide();
            return;
          } else {
            _msg.hide();
          }
        }
      }
    };

    _Messenger.prototype.newMessage = function(opts) {
      var msg, _ref2, _ref3, _ref4,
        _this = this;
      if (opts == null) {
        opts = {};
      }
      opts.messenger = this;
      _Message = (_ref2 = (_ref3 = Messenger.themes[(_ref4 = opts.theme) != null ? _ref4 : this.options.theme]) != null ? _ref3.Message : void 0) != null ? _ref2 : RetryingMessage;
      msg = new _Message(opts);
      msg.on('show', function() {
        if (opts.scrollTo && _this.$el.css('position') !== 'fixed') {
          return msg.scrollTo();
        }
      });
      msg.on('hide show render', this.updateMessageSlotClasses, this);
      return msg;
    };

    _Messenger.prototype.updateMessageSlotClasses = function() {
      var anyShown, last, rec, willBeFirst, _i, _len, _ref2;
      willBeFirst = true;
      last = null;
      anyShown = false;
      _ref2 = this.history;
      for (_i = 0, _len = _ref2.length; _i < _len; _i++) {
        rec = _ref2[_i];
        rec.$slot.removeClass('messenger-first messenger-last messenger-shown');
        if (rec.msg.shown && rec.msg.rendered) {
          rec.$slot.addClass('messenger-shown');
          anyShown = true;
          last = rec;
          if (willBeFirst) {
            willBeFirst = false;
            rec.$slot.addClass('messenger-first');
          }
        }
      }
      if (last != null) {
        last.$slot.addClass('messenger-last');
      }
      return this.$el["" + (anyShown ? 'remove' : 'add') + "Class"]('messenger-empty');
    };

    _Messenger.prototype.hideAll = function() {
      var rec, _i, _len, _ref2, _results;
      _ref2 = this.history;
      _results = [];
      for (_i = 0, _len = _ref2.length; _i < _len; _i++) {
        rec = _ref2[_i];
        _results.push(rec.msg.hide());
      }
      return _results;
    };

    _Messenger.prototype.post = function(opts) {
      var msg;
      if (_.isString(opts)) {
        opts = {
          message: opts
        };
      }
      opts = $.extend(true, {}, this.messageDefaults, opts);
      msg = this.newMessage(opts);
      msg.update(opts);
      return msg;
    };

    return _Messenger;

  })(BaseView);

  ActionMessenger = (function(_super) {

    __extends(ActionMessenger, _super);

    function ActionMessenger() {
      return ActionMessenger.__super__.constructor.apply(this, arguments);
    }

    ActionMessenger.prototype.doDefaults = {
      progressMessage: null,
      successMessage: null,
      errorMessage: "Error connecting to the server.",
      showSuccessWithoutError: true,
      retry: {
        auto: true,
        allow: true
      },
      action: $.ajax
    };

    ActionMessenger.prototype.hookBackboneAjax = function(msgr_opts) {
      var _ajax,
        _this = this;
      if (msgr_opts == null) {
        msgr_opts = {};
      }
      if (!(window.Backbone != null)) {
        throw 'Expected Backbone to be defined';
      }
      msgr_opts = _.defaults(msgr_opts, {
        id: 'BACKBONE_ACTION',
        errorMessage: false,
        successMessage: "Request completed successfully.",
        showSuccessWithoutError: false
      });
      _ajax = function(options) {
        var sync_msgr_opts;
        sync_msgr_opts = _.extend({}, msgr_opts, options.messenger);
        return _this["do"](sync_msgr_opts, options);
      };
      if (Backbone.ajax != null) {
        if (Backbone.ajax._withoutMessenger) {
          Backbone.ajax = Backbone.ajax._withoutMessenger;
        }
        if (!(msgr_opts.action != null) || msgr_opts.action === this.doDefaults.action) {
          msgr_opts.action = Backbone.ajax;
        }
        _ajax._withoutMessenger = Backbone.ajax;
        return Backbone.ajax = _ajax;
      } else {
        return Backbone.sync = _.wrap(Backbone.sync, function() {
          var args, _old_ajax, _old_sync;
          _old_sync = arguments[0], args = 2 <= arguments.length ? __slice.call(arguments, 1) : [];
          _old_ajax = $.ajax;
          $.ajax = _ajax;
          _old_sync.call.apply(_old_sync, [this].concat(__slice.call(args)));
          return $.ajax = _old_ajax;
        });
      }
    };

    ActionMessenger.prototype._getHandlerResponse = function(returnVal) {
      if (returnVal === false) {
        return false;
      }
      if (returnVal === true || !(returnVal != null)) {
        return true;
      }
      return returnVal;
    };

    ActionMessenger.prototype._parseEvents = function(events) {
      var desc, firstSpace, func, label, out, type, _ref2;
      if (events == null) {
        events = {};
      }
      out = {};
      for (label in events) {
        func = events[label];
        firstSpace = label.indexOf(' ');
        type = label.substring(0, firstSpace);
        desc = label.substring(firstSpace + 1);
        if ((_ref2 = out[type]) == null) {
          out[type] = {};
        }
        out[type][desc] = func;
      }
      return out;
    };

    ActionMessenger.prototype._normalizeResponse = function() {
      var data, elem, resp, type, xhr, _i, _len;
      resp = 1 <= arguments.length ? __slice.call(arguments, 0) : [];
      type = null;
      xhr = null;
      data = null;
      for (_i = 0, _len = resp.length; _i < _len; _i++) {
        elem = resp[_i];
        if (elem === 'success' || elem === 'timeout' || elem === 'abort') {
          type = elem;
        } else if (((elem != null ? elem.readyState : void 0) != null) && ((elem != null ? elem.responseText : void 0) != null)) {
          xhr = elem;
        } else if (_.isObject(elem)) {
          data = elem;
        }
      }
      return [type, data, xhr];
    };

    ActionMessenger.prototype.run = function() {
      var args, attr, events, getMessageText, handler, handlers, m_opts, msg, old, opts, promiseAttrs, type, _i, _len, _ref2, _ref3,
        _this = this;
      m_opts = arguments[0], opts = arguments[1], args = 3 <= arguments.length ? __slice.call(arguments, 2) : [];
      if (opts == null) {
        opts = {};
      }
      m_opts = $.extend(true, {}, this.messageDefaults, this.doDefaults, m_opts != null ? m_opts : {});
      events = this._parseEvents(m_opts.events);
      getMessageText = function(type, xhr) {
        var message;
        message = m_opts[type + 'Message'];
        if (_.isFunction(message)) {
          return message.call(_this, type, xhr);
        }
        return message;
      };
      msg = (_ref2 = m_opts.messageInstance) != null ? _ref2 : this.newMessage(m_opts);
      if (m_opts.id != null) {
        msg.options.id = m_opts.id;
      }
      if (m_opts.progressMessage != null) {
        msg.update($.extend({}, m_opts, {
          message: getMessageText('progress', null),
          type: 'info'
        }));
      }
      handlers = {};
      _.each(['error', 'success'], function(type) {
        return handlers[type] = function() {
          var data, defaultOpts, handlerResp, msgOpts, reason, resp, responseOpts, xhr, _base, _ref10, _ref3, _ref4, _ref5, _ref6, _ref7, _ref8, _ref9;
          resp = 1 <= arguments.length ? __slice.call(arguments, 0) : [];
          if ((_ref3 = opts[type]) != null ? _ref3._originalHandler : void 0) {
            opts[type] = opts[type]._originalHandler;
          }
          _ref4 = _this._normalizeResponse.apply(_this, resp), reason = _ref4[0], data = _ref4[1], xhr = _ref4[2];
          if (type === 'success' && !(msg.errorCount != null) && m_opts.showSuccessWithoutError === false) {
            m_opts['successMessage'] = null;
          }
          if (type === 'error') {
            if ((_ref5 = m_opts.errorCount) == null) {
              m_opts.errorCount = 0;
            }
            m_opts.errorCount += 1;
          }
          handlerResp = m_opts.returnsPromise ? resp[0] : typeof (_base = opts[type])._originalHandler === "function" ? _base._originalHandler.apply(_base, resp) : void 0;
          responseOpts = _this._getHandlerResponse(handlerResp);
          if (_.isString(responseOpts)) {
            responseOpts = {
              message: responseOpts
            };
          }
          if (type === 'error' && ((xhr != null ? xhr.status : void 0) === 0 || reason === 'abort')) {
            msg.hide();
            return;
          }
          if (type === 'error' && ((m_opts.ignoredErrorCodes != null) && (_ref6 = xhr != null ? xhr.status : void 0, __indexOf.call(m_opts.ignoredErrorCodes, _ref6) >= 0))) {
            msg.hide();
            return;
          }
          defaultOpts = {
            message: getMessageText(type, xhr),
            type: type,
            events: (_ref7 = events[type]) != null ? _ref7 : {},
            hideOnNavigate: type === 'success'
          };
          msgOpts = $.extend({}, m_opts, defaultOpts, responseOpts);
          if (typeof ((_ref8 = msgOpts.retry) != null ? _ref8.allow : void 0) === 'number') {
            msgOpts.retry.allow--;
          }
          if (type === 'error' && (xhr != null ? xhr.status : void 0) >= 500 && ((_ref9 = msgOpts.retry) != null ? _ref9.allow : void 0)) {
            if (msgOpts.retry.delay == null) {
              if (msgOpts.errorCount < 4) {
                msgOpts.retry.delay = 10;
              } else {
                msgOpts.retry.delay = 5 * 60;
              }
            }
            if (msgOpts.hideAfter) {
              if ((_ref10 = msgOpts._hideAfter) == null) {
                msgOpts._hideAfter = msgOpts.hideAfter;
              }
              msgOpts.hideAfter = msgOpts._hideAfter + msgOpts.retry.delay;
            }
            msgOpts._retryActions = true;
            msgOpts.actions = {
              retry: {
                label: 'retry now',
                phrase: 'Retrying TIME',
                auto: msgOpts.retry.auto,
                delay: msgOpts.retry.delay,
                action: function() {
                  msgOpts.messageInstance = msg;
                  return setTimeout(function() {
                    return _this["do"].apply(_this, [msgOpts, opts].concat(__slice.call(args)));
                  }, 0);
                }
              },
              cancel: {
                action: function() {
                  return msg.cancel();
                }
              }
            };
          } else if (msgOpts._retryActions) {
            delete msgOpts.actions.retry;
            delete msgOpts.actions.cancel;
            delete m_opts._retryActions;
          }
          msg.update(msgOpts);
          if (responseOpts && msgOpts.message) {
            Messenger();
            return msg.show();
          } else {
            return msg.hide();
          }
        };
      });
      if (!m_opts.returnsPromise) {
        for (type in handlers) {
          handler = handlers[type];
          old = opts[type];
          opts[type] = handler;
          opts[type]._originalHandler = old;
        }
      }
      msg._actionInstance = m_opts.action.apply(m_opts, [opts].concat(__slice.call(args)));
      if (m_opts.returnsPromise) {
        msg._actionInstance.then(handlers.success, handlers.error);
      }
      promiseAttrs = ['done', 'progress', 'fail', 'state', 'then'];
      for (_i = 0, _len = promiseAttrs.length; _i < _len; _i++) {
        attr = promiseAttrs[_i];
        if (msg[attr] != null) {
          delete msg[attr];
        }
        msg[attr] = (_ref3 = msg._actionInstance) != null ? _ref3[attr] : void 0;
      }
      return msg;
    };

    ActionMessenger.prototype["do"] = ActionMessenger.prototype.run;

    ActionMessenger.prototype.ajax = function() {
      var args, m_opts;
      m_opts = arguments[0], args = 2 <= arguments.length ? __slice.call(arguments, 1) : [];
      m_opts.action = $.ajax;
      return this.run.apply(this, [m_opts].concat(__slice.call(args)));
    };

    ActionMessenger.prototype.expectPromise = function(action, m_opts) {
      m_opts = _.extend({}, m_opts, {
        action: action,
        returnsPromise: true
      });
      return this.run(m_opts);
    };

    return ActionMessenger;

  })(_Messenger);

  $.fn.messenger = function() {
    var $el, args, func, instance, opts, _ref2, _ref3, _ref4;
    func = arguments[0], args = 2 <= arguments.length ? __slice.call(arguments, 1) : [];
    if (func == null) {
      func = {};
    }
    $el = this;
    if (!(func != null) || !_.isString(func)) {
      opts = func;
      if (!($el.data('messenger') != null)) {
        _Messenger = (_ref2 = (_ref3 = Messenger.themes[opts.theme]) != null ? _ref3.Messenger : void 0) != null ? _ref2 : ActionMessenger;
        $el.data('messenger', instance = new _Messenger($.extend({
          el: $el
        }, opts)));
        instance.render();
      }
      return $el.data('messenger');
    } else {
      return (_ref4 = $el.data('messenger'))[func].apply(_ref4, args);
    }
  };

  window.Messenger._call = function(opts) {
    var $el, $parent, choosen_loc, chosen_loc, classes, defaultOpts, inst, loc, locations, _i, _len;
    defaultOpts = {
      extraClasses: 'messenger-fixed messenger-on-bottom messenger-on-right',
      theme: 'future',
      maxMessages: 9,
      parentLocations: ['body']
    };
    opts = $.extend(defaultOpts, $._messengerDefaults, Messenger.options, opts);
    if (opts.theme != null) {
      opts.extraClasses += " messenger-theme-" + opts.theme;
    }
    inst = opts.instance || Messenger.instance;
    if (opts.instance == null) {
      locations = opts.parentLocations;
      $parent = null;
      choosen_loc = null;
      for (_i = 0, _len = locations.length; _i < _len; _i++) {
        loc = locations[_i];
        $parent = $(loc);
        if ($parent.length) {
          chosen_loc = loc;
          break;
        }
      }
      if (!inst) {
        $el = $('<ul>');
        $parent.prepend($el);
        inst = $el.messenger(opts);
        inst._location = chosen_loc;
        Messenger.instance = inst;
      } else if ($(inst._location) !== $(chosen_loc)) {
        inst.$el.detach();
        $parent.prepend(inst.$el);
      }
    }
    if (inst._addedClasses != null) {
      inst.$el.removeClass(inst._addedClasses);
    }
    inst.$el.addClass(classes = "" + inst.className + " " + opts.extraClasses);
    inst._addedClasses = classes;
    return inst;
  };

  $.extend(Messenger, {
    Message: RetryingMessage,
    Messenger: ActionMessenger,
    themes: (_ref2 = Messenger.themes) != null ? _ref2 : {}
  });

  $.globalMessenger = window.Messenger = Messenger;

}).call(this);
