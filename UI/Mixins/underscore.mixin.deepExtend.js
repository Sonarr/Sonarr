/**
 * Underscore mixins for deep objects
 *
 * Based on https://gist.github.com/echong/3861963
 */
(function() {
    var arrays, basicObjects, deepClone, deepExtend, deepExtendCouple, isBasicObject,
        __slice = [].slice;

    deepClone = function(obj) {
        var func, isArr;
        if (!_.isObject(obj) || _.isFunction(obj)) {
            return obj;
        }
        if (obj instanceof Backbone.Collection || obj instanceof Backbone.Model) {
            return obj;
        }
        if (_.isDate(obj)) {
            return new Date(obj.getTime());
        }
        if (_.isRegExp(obj)) {
            return new RegExp(obj.source, obj.toString().replace(/.*\//, ""));
        }
        isArr = _.isArray(obj || _.isArguments(obj));
        func = function(memo, value, key) {
            if (isArr) {
                memo.push(deepClone(value));
            } else {
                memo[key] = deepClone(value);
            }
            return memo;
        };
        return _.reduce(obj, func, isArr ? [] : {});
    };

    isBasicObject = function(object) {
        if (object == null) return false;
        return (object.prototype === {}.prototype || object.prototype === Object.prototype) && _.isObject(object) && !_.isArray(object) && !_.isFunction(object) && !_.isDate(object) && !_.isRegExp(object) && !_.isArguments(object);
    };

    basicObjects = function(object) {
        return _.filter(_.keys(object), function(key) {
            return isBasicObject(object[key]);
        });
    };

    arrays = function(object) {
        return _.filter(_.keys(object), function(key) {
            return _.isArray(object[key]);
        });
    };

    deepExtendCouple = function(destination, source, maxDepth) {
        var combine, recurse, sharedArrayKey, sharedArrayKeys, sharedObjectKey, sharedObjectKeys, _i, _j, _len, _len1;
        if (maxDepth == null) {
            maxDepth = 20;
        }
        if (maxDepth <= 0) {
            console.warn('_.deepExtend(): Maximum depth of recursion hit.');
            return _.extend(destination, source);
        }
        sharedObjectKeys = _.intersection(basicObjects(destination), basicObjects(source));
        recurse = function(key) {
            return source[key] = deepExtendCouple(destination[key], source[key], maxDepth - 1);
        };
        for (_i = 0, _len = sharedObjectKeys.length; _i < _len; _i++) {
            sharedObjectKey = sharedObjectKeys[_i];
            recurse(sharedObjectKey);
        }
        sharedArrayKeys = _.intersection(arrays(destination), arrays(source));
        combine = function(key) {
            return source[key] = _.union(destination[key], source[key]);
        };
        for (_j = 0, _len1 = sharedArrayKeys.length; _j < _len1; _j++) {
            sharedArrayKey = sharedArrayKeys[_j];
            combine(sharedArrayKey);
        }
        return _.extend(destination, source);
    };

    deepExtend = function() {
        var finalObj, maxDepth, objects, _i;
        objects = 2 <= arguments.length ? __slice.call(arguments, 0, _i = arguments.length - 1) : (_i = 0, []), maxDepth = arguments[_i++];
        if (!_.isNumber(maxDepth)) {
            objects.push(maxDepth);
            maxDepth = 20;
        }
        if (objects.length <= 1) {
            return objects[0];
        }
        if (maxDepth <= 0) {
            return _.extend.apply(this, objects);
        }
        finalObj = objects.shift();
        while (objects.length > 0) {
            finalObj = deepExtendCouple(finalObj, deepClone(objects.shift()), maxDepth);
        }
        return finalObj;
    };

    require(['underscore'], function (_) {

        _.mixin({
            deepClone    : deepClone,
            isBasicObject: isBasicObject,
            basicObjects : basicObjects,
            arrays       : arrays,
            deepExtend   : deepExtend
        });
    });

}).call(this);
