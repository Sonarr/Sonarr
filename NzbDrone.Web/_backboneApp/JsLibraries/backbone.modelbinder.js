// Backbone.ModelBinder v0.1.6
// (c) 2012 Bart Wood
// Distributed Under MIT License

(function (factory) {
    if (typeof define === 'function' && define.amd) {
        // AMD. Register as an anonymous module.
        define(['underscore', 'jquery', 'backbone'], factory);
    } else {
        // Browser globals
        factory(_, $, Backbone);
    }
}(function (_, $, Backbone) {

    if (!Backbone) {
        throw 'Please include Backbone.js before Backbone.ModelBinder.js';
    }

    Backbone.ModelBinder = function (modelSetOptions) {
        _.bindAll(this);
        this._modelSetOptions = modelSetOptions || {};
    };

    // Current version of the library.
    Backbone.ModelBinder.VERSION = '0.1.6';
    Backbone.ModelBinder.Constants = {};
    Backbone.ModelBinder.Constants.ModelToView = 'ModelToView';
    Backbone.ModelBinder.Constants.ViewToModel = 'ViewToModel';

    _.extend(Backbone.ModelBinder.prototype, {

        bind: function (model, rootEl, attributeBindings, modelSetOptions) {
            this.unbind();

            this._model = model;
            this._rootEl = rootEl;
            this._modelSetOptions = _.extend({}, this._modelSetOptions, modelSetOptions);

            if (!this._model) throw 'model must be specified';
            if (!this._rootEl) throw 'rootEl must be specified';

            if (attributeBindings) {
                // Create a deep clone of the attribute bindings
                this._attributeBindings = $.extend(true, {}, attributeBindings);

                this._initializeAttributeBindings();
                this._initializeElBindings();
            }
            else {
                this._initializeDefaultBindings();
            }

            this._bindModelToView();
            this._bindViewToModel();
        },

        bindCustomTriggers: function (model, rootEl, triggers, attributeBindings, modelSetOptions) {
            this._triggers = triggers;
            this.bind(model, rootEl, attributeBindings, modelSetOptions)
        },

        unbind: function () {
            this._unbindModelToView();
            this._unbindViewToModel();

            if (this._attributeBindings) {
                delete this._attributeBindings;
                this._attributeBindings = undefined;
            }
        },

        // Converts the input bindings, which might just be empty or strings, to binding objects
        _initializeAttributeBindings: function () {
            var attributeBindingKey, inputBinding, attributeBinding, elementBindingCount, elementBinding;

            for (attributeBindingKey in this._attributeBindings) {
                inputBinding = this._attributeBindings[attributeBindingKey];

                if (_.isString(inputBinding)) {
                    attributeBinding = { elementBindings: [{ selector: inputBinding }] };
                }
                else if (_.isArray(inputBinding)) {
                    attributeBinding = { elementBindings: inputBinding };
                }
                else if (_.isObject(inputBinding)) {
                    attributeBinding = { elementBindings: [inputBinding] };
                }
                else {
                    throw 'Unsupported type passed to Model Binder ' + attributeBinding;
                }

                // Add a linkage from the element binding back to the attribute binding
                for (elementBindingCount = 0; elementBindingCount < attributeBinding.elementBindings.length; elementBindingCount++) {
                    elementBinding = attributeBinding.elementBindings[elementBindingCount];
                    elementBinding.attributeBinding = attributeBinding;
                }

                attributeBinding.attributeName = attributeBindingKey;
                this._attributeBindings[attributeBindingKey] = attributeBinding;
            }
        },

        // If the bindings are not specified, the default binding is performed on the name attribute
        _initializeDefaultBindings: function () {
            var elCount, namedEls, namedEl, name, attributeBinding;
            this._attributeBindings = {};
            namedEls = $('[name]', this._rootEl);

            for (elCount = 0; elCount < namedEls.length; elCount++) {
                namedEl = namedEls[elCount];
                name = $(namedEl).attr('name');

                // For elements like radio buttons we only want a single attribute binding with possibly multiple element bindings
                if (!this._attributeBindings[name]) {
                    attributeBinding = { attributeName: name };
                    attributeBinding.elementBindings = [{ attributeBinding: attributeBinding, boundEls: [namedEl] }];
                    this._attributeBindings[name] = attributeBinding;
                }
                else {
                    this._attributeBindings[name].elementBindings.push({ attributeBinding: this._attributeBindings[name], boundEls: [namedEl] });
                }
            }
        },

        _initializeElBindings: function () {
            var bindingKey, attributeBinding, bindingCount, elementBinding, foundEls, elCount, el;
            for (bindingKey in this._attributeBindings) {
                attributeBinding = this._attributeBindings[bindingKey];

                for (bindingCount = 0; bindingCount < attributeBinding.elementBindings.length; bindingCount++) {
                    elementBinding = attributeBinding.elementBindings[bindingCount];
                    if (elementBinding.selector === '') {
                        foundEls = $(this._rootEl);
                    }
                    else {
                        foundEls = $(elementBinding.selector, this._rootEl);
                    }

                    if (foundEls.length === 0) {
                        throw 'Bad binding found. No elements returned for binding selector ' + elementBinding.selector;
                    }
                    else {
                        elementBinding.boundEls = [];
                        for (elCount = 0; elCount < foundEls.length; elCount++) {
                            el = foundEls[elCount];
                            elementBinding.boundEls.push(el);
                        }
                    }
                }
            }
        },

        _bindModelToView: function () {
            this._model.on('change', this._onModelChange, this);

            this.copyModelAttributesToView();
        },

        // attributesToCopy is an optional parameter - if empty, all attributes
        // that are bound will be copied.  Otherwise, only attributeBindings specified
        // in the attributesToCopy are copied.
        copyModelAttributesToView: function (attributesToCopy) {
            var attributeName, attributeBinding;

            for (attributeName in this._attributeBindings) {
                if (attributesToCopy === undefined || _.indexOf(attributesToCopy, attributeName) !== -1) {
                    attributeBinding = this._attributeBindings[attributeName];
                    this._copyModelToView(attributeBinding);
                }
            }
        },

        _unbindModelToView: function () {
            if (this._model) {
                this._model.off('change', this._onModelChange);
                this._model = undefined;
            }
        },

        _bindViewToModel: function () {
            if (this._triggers) {
                _.each(this._triggers, function (event, selector) {
                    $(this._rootEl).delegate(selector, event, this._onElChanged);
                }, this);
            }
            else {
                $(this._rootEl).delegate('', 'change', this._onElChanged);
                // The change event doesn't work properly for contenteditable elements - but blur does
                $(this._rootEl).delegate('[contenteditable]', 'blur', this._onElChanged);
            }
        },

        _unbindViewToModel: function () {
            if (this._rootEl) {
                if (this._triggers) {
                    _.each(this._triggers, function (event, selector) {
                        $(this._rootEl).undelegate(selector, event, this._onElChanged);
                    }, this);
                }
                else {
                    $(this._rootEl).undelegate('', 'change', this._onElChanged);
                    $(this._rootEl).undelegate('[contenteditable]', 'blur', this._onElChanged);
                }
            }
        },

        _onElChanged: function (event) {
            var el, elBindings, elBindingCount, elBinding;

            el = $(event.target)[0];
            elBindings = this._getElBindings(el);

            for (elBindingCount = 0; elBindingCount < elBindings.length; elBindingCount++) {
                elBinding = elBindings[elBindingCount];
                if (this._isBindingUserEditable(elBinding)) {
                    this._copyViewToModel(elBinding, el);
                }
            }
        },

        _isBindingUserEditable: function (elBinding) {
            return elBinding.elAttribute === undefined ||
                elBinding.elAttribute === 'text' ||
                elBinding.elAttribute === 'html';
        },

        _getElBindings: function (findEl) {
            var attributeName, attributeBinding, elementBindingCount, elementBinding, boundElCount, boundEl;
            var elBindings = [];

            for (attributeName in this._attributeBindings) {
                attributeBinding = this._attributeBindings[attributeName];

                for (elementBindingCount = 0; elementBindingCount < attributeBinding.elementBindings.length; elementBindingCount++) {
                    elementBinding = attributeBinding.elementBindings[elementBindingCount];

                    for (boundElCount = 0; boundElCount < elementBinding.boundEls.length; boundElCount++) {
                        boundEl = elementBinding.boundEls[boundElCount];

                        if (boundEl === findEl) {
                            elBindings.push(elementBinding);
                        }
                    }
                }
            }

            return elBindings;
        },

        _onModelChange: function () {
            var changedAttribute, attributeBinding;

            for (changedAttribute in this._model.changedAttributes()) {
                attributeBinding = this._attributeBindings[changedAttribute];

                if (attributeBinding) {
                    this._copyModelToView(attributeBinding);
                }
            }
        },

        _copyModelToView: function (attributeBinding) {
            var elementBindingCount, elementBinding, boundElCount, boundEl, value, convertedValue;

            value = this._model.get(attributeBinding.attributeName);

            for (elementBindingCount = 0; elementBindingCount < attributeBinding.elementBindings.length; elementBindingCount++) {
                elementBinding = attributeBinding.elementBindings[elementBindingCount];

                for (boundElCount = 0; boundElCount < elementBinding.boundEls.length; boundElCount++) {
                    boundEl = elementBinding.boundEls[boundElCount];

                    if (!boundEl._isSetting) {
                        convertedValue = this._getConvertedValue(Backbone.ModelBinder.Constants.ModelToView, elementBinding, value);
                        this._setEl($(boundEl), elementBinding, convertedValue);
                    }
                }
            }
        },

        _setEl: function (el, elementBinding, convertedValue) {
            if (elementBinding.elAttribute) {
                this._setElAttribute(el, elementBinding, convertedValue);
            }
            else {
                this._setElValue(el, convertedValue);
            }
        },

        _setElAttribute: function (el, elementBinding, convertedValue) {
            switch (elementBinding.elAttribute) {
                case 'html':
                    el.html(convertedValue);
                    break;
                case 'text':
                    el.text(convertedValue);
                    break;
                case 'enabled':
                    el.attr('disabled', !convertedValue);
                    break;
                case 'displayed':
                    el[convertedValue ? 'show' : 'hide']();
                    break;
                case 'hidden':
                    el[convertedValue ? 'hide' : 'show']();
                    break;
                case 'css':
                    el.css(elementBinding.cssAttribute, convertedValue);
                    break;
                case 'class':
                    var previousValue = this._model.previous(elementBinding.attributeBinding.attributeName);
                    if (!_.isUndefined(previousValue)) {
                        previousValue = this._getConvertedValue(Backbone.ModelBinder.Constants.ModelToView, elementBinding, previousValue);
                        el.removeClass(previousValue);
                    }

                    if (convertedValue) {
                        el.addClass(convertedValue);
                    }
                    break;
                default:
                    el.attr(elementBinding.elAttribute, convertedValue);
            }
        },

        _setElValue: function (el, convertedValue) {
            if (el.attr('type')) {
                switch (el.attr('type')) {
                    case 'radio':
                        if (el.val() === convertedValue) {
                            el.attr('checked', 'checked');
                        }
                        break;
                    case 'checkbox':
                        if (convertedValue) {
                            el.attr('checked', 'checked');
                        }
                        else {
                            el.removeAttr('checked');
                        }
                        break;
                    default:
                        el.val(convertedValue);
                }
            }
            else if (el.is('input') || el.is('select') || el.is('textarea')) {
                el.val(convertedValue);
            }
            else {
                el.text(convertedValue);
            }
        },

        _copyViewToModel: function (elementBinding, el) {
            var value, convertedValue;

            if (!el._isSetting) {

                el._isSetting = true;
                this._setModel(elementBinding, $(el));
                el._isSetting = false;

                if (elementBinding.converter) {
                    value = this._model.get(elementBinding.attributeBinding.attributeName);
                    convertedValue = this._getConvertedValue(Backbone.ModelBinder.Constants.ModelToView, elementBinding, value);
                    this._setEl($(el), elementBinding, convertedValue);
                }
            }
        },

        _getElValue: function (elementBinding, el) {
            switch (el.attr('type')) {
                case 'checkbox':
                    return el.prop('checked') ? true : false;
                default:
                    if (el.attr('contenteditable') !== undefined) {
                        return el.html();
                    }
                    else {
                        return el.val();
                    }
            }
        },

        _setModel: function (elementBinding, el) {
            var data = {};
            var elVal = this._getElValue(elementBinding, el);
            elVal = this._getConvertedValue(Backbone.ModelBinder.Constants.ViewToModel, elementBinding, elVal);
            data[elementBinding.attributeBinding.attributeName] = elVal;
            var opts = _.extend({}, this._modelSetOptions, { changeSource: 'ModelBinder' });
            this._model.set(data, opts);
        },

        _getConvertedValue: function (direction, elementBinding, value) {
            if (elementBinding.converter) {
                value = elementBinding.converter(direction, value, elementBinding.attributeBinding.attributeName, this._model);
            }

            return value;
        }
    });

    Backbone.ModelBinder.CollectionConverter = function (collection) {
        this._collection = collection;

        if (!this._collection) {
            throw 'Collection must be defined';
        }
        _.bindAll(this, 'convert');
    };

    _.extend(Backbone.ModelBinder.CollectionConverter.prototype, {
        convert: function (direction, value) {
            if (direction === Backbone.ModelBinder.Constants.ModelToView) {
                return value ? value.id : undefined;
            }
            else {
                return this._collection.get(value);
            }
        }
    });

    // A static helper function to create a default set of bindings that you can customize before calling the bind() function
    // rootEl - where to find all of the bound elements
    // attributeType - probably 'name' or 'id' in most cases
    // converter(optional) - the default converter you want applied to all your bindings
    // elAttribute(optional) - the default elAttribute you want applied to all your bindings
    Backbone.ModelBinder.createDefaultBindings = function (rootEl, attributeType, converter, elAttribute) {
        var foundEls, elCount, foundEl, attributeName;
        var bindings = {};

        foundEls = $('[' + attributeType + ']', rootEl);

        for (elCount = 0; elCount < foundEls.length; elCount++) {
            foundEl = foundEls[elCount];
            attributeName = $(foundEl).attr(attributeType);

            if (!bindings[attributeName]) {
                var attributeBinding = { selector: '[' + attributeType + '="' + attributeName + '"]' };
                bindings[attributeName] = attributeBinding;

                if (converter) {
                    bindings[attributeName].converter = converter;
                }

                if (elAttribute) {
                    bindings[attributeName].elAttribute = elAttribute;
                }
            }
        }

        return bindings;
    };

    // Helps you to combine 2 sets of bindings
    Backbone.ModelBinder.combineBindings = function (destination, source) {
        _.each(source, function (value, key) {
            var elementBinding = { selector: value.selector };

            if (value.converter) {
                elementBinding.converter = value.converter;
            }

            if (value.elAttribute) {
                elementBinding.elAttribute = value.elAttribute;
            }

            if (!destination[key]) {
                destination[key] = elementBinding;
            }
            else {
                destination[key] = [destination[key], elementBinding];
            }
        });

        return destination;
    };


    return Backbone.ModelBinder;

}));