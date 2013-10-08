// Backbone.ModelBinder v1.0.2
// (c) 2013 Bart Wood
// Distributed Under MIT License

(function (factory) {
    if (typeof define === 'function' && define.amd) {
        // AMD. Register as an anonymous module.
        define(['underscore', 'jquery', 'backbone'], factory);
    } else {
        // Browser globals
        factory(_, $, Backbone);
    }
}(function(_, $, Backbone){

    if(!Backbone){
        throw 'Please include Backbone.js before Backbone.ModelBinder.js';
    }

    Backbone.ModelBinder = function(){
        _.bindAll.apply(_, [this].concat(_.functions(this)));
    };

    // Static setter for class level options
    Backbone.ModelBinder.SetOptions = function(options){
        Backbone.ModelBinder.options = options;
    };

    // Current version of the library.
    Backbone.ModelBinder.VERSION = '1.0.2';
    Backbone.ModelBinder.Constants = {};
    Backbone.ModelBinder.Constants.ModelToView = 'ModelToView';
    Backbone.ModelBinder.Constants.ViewToModel = 'ViewToModel';

    _.extend(Backbone.ModelBinder.prototype, {

        bind:function (model, rootEl, attributeBindings, options) {
            this.unbind();

            this._model = model;
            this._rootEl = rootEl;
            this._setOptions(options);

            if (!this._model) this._throwException('model must be specified');
            if (!this._rootEl) this._throwException('rootEl must be specified');

            if(attributeBindings){
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

        unbind:function () {
            this._unbindModelToView();
            this._unbindViewToModel();

            if(this._attributeBindings){
                delete this._attributeBindings;
                this._attributeBindings = undefined;
            }
        },

        _setOptions: function(options){
            this._options = _.extend({
                boundAttribute: 'name'
            }, Backbone.ModelBinder.options, options);

            // initialize default options
            if(!this._options['modelSetOptions']){
                this._options['modelSetOptions'] = {};
            }
            this._options['modelSetOptions'].changeSource = 'ModelBinder';

            if(!this._options['changeTriggers']){
                this._options['changeTriggers'] = {'': 'change', '[contenteditable]': 'blur'};
            }

            if(!this._options['initialCopyDirection']){
                this._options['initialCopyDirection'] = Backbone.ModelBinder.Constants.ModelToView;
            }
        },

        // Converts the input bindings, which might just be empty or strings, to binding objects
        _initializeAttributeBindings:function () {
            var attributeBindingKey, inputBinding, attributeBinding, elementBindingCount, elementBinding;

            for (attributeBindingKey in this._attributeBindings) {
                inputBinding = this._attributeBindings[attributeBindingKey];

                if (_.isString(inputBinding)) {
                    attributeBinding = {elementBindings: [{selector: inputBinding}]};
                }
                else if (_.isArray(inputBinding)) {
                    attributeBinding = {elementBindings: inputBinding};
                }
                else if(_.isObject(inputBinding)){
                    attributeBinding = {elementBindings: [inputBinding]};
                }
                else {
                    this._throwException('Unsupported type passed to Model Binder ' + attributeBinding);
                }

                // Add a linkage from the element binding back to the attribute binding
                for(elementBindingCount = 0; elementBindingCount < attributeBinding.elementBindings.length; elementBindingCount++){
                    elementBinding = attributeBinding.elementBindings[elementBindingCount];
                    elementBinding.attributeBinding = attributeBinding;
                }

                attributeBinding.attributeName = attributeBindingKey;
                this._attributeBindings[attributeBindingKey] = attributeBinding;
            }
        },

        // If the bindings are not specified, the default binding is performed on the specified attribute, name by default
        _initializeDefaultBindings: function(){
            var elCount, elsWithAttribute, matchedEl, name, attributeBinding;

            this._attributeBindings = {};
            elsWithAttribute = $('[' + this._options['boundAttribute'] + ']', this._rootEl);

            for(elCount = 0; elCount < elsWithAttribute.length; elCount++){
                matchedEl = elsWithAttribute[elCount];
                name = $(matchedEl).attr(this._options['boundAttribute']);

                // For elements like radio buttons we only want a single attribute binding with possibly multiple element bindings
                if(!this._attributeBindings[name]){
                    attributeBinding =  {attributeName: name};
                    attributeBinding.elementBindings = [{attributeBinding: attributeBinding, boundEls: [matchedEl]}];
                    this._attributeBindings[name] = attributeBinding;
                }
                else{
                    this._attributeBindings[name].elementBindings.push({attributeBinding: this._attributeBindings[name], boundEls: [matchedEl]});
                }
            }
        },

        _initializeElBindings:function () {
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
                        this._throwException('Bad binding found. No elements returned for binding selector ' + elementBinding.selector);
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

            if(this._options['initialCopyDirection'] === Backbone.ModelBinder.Constants.ModelToView){
                this.copyModelAttributesToView();
            }
        },

        // attributesToCopy is an optional parameter - if empty, all attributes
        // that are bound will be copied.  Otherwise, only attributeBindings specified
        // in the attributesToCopy are copied.
        copyModelAttributesToView: function(attributesToCopy){
            var attributeName, attributeBinding;

            for (attributeName in this._attributeBindings) {
                if(attributesToCopy === undefined || _.indexOf(attributesToCopy, attributeName) !== -1){
                    attributeBinding = this._attributeBindings[attributeName];
                    this._copyModelToView(attributeBinding);
                }
            }
        },

        copyViewValuesToModel: function(){
            var bindingKey, attributeBinding, bindingCount, elementBinding, elCount, el;
            for (bindingKey in this._attributeBindings) {
                attributeBinding = this._attributeBindings[bindingKey];

                for (bindingCount = 0; bindingCount < attributeBinding.elementBindings.length; bindingCount++) {
                    elementBinding = attributeBinding.elementBindings[bindingCount];

                    if(this._isBindingUserEditable(elementBinding)){
                        if(this._isBindingRadioGroup(elementBinding)){
                            el = this._getRadioButtonGroupCheckedEl(elementBinding);
                            if(el){
                                this._copyViewToModel(elementBinding, el);
                            }
                        }
                        else {
                            for(elCount = 0; elCount < elementBinding.boundEls.length; elCount++){
                                el = $(elementBinding.boundEls[elCount]);
                                if(this._isElUserEditable(el)){
                                    this._copyViewToModel(elementBinding, el);
                                }
                            }
                        }
                    }
                }
            }
        },

        _unbindModelToView: function(){
            if(this._model){
                this._model.off('change', this._onModelChange);
                this._model = undefined;
            }
        },

        _bindViewToModel: function () {
            _.each(this._options['changeTriggers'], function (event, selector) {
                $(this._rootEl).delegate(selector, event, this._onElChanged);
            }, this);

            if(this._options['initialCopyDirection'] === Backbone.ModelBinder.Constants.ViewToModel){
                this.copyViewValuesToModel();
            }
        },

        _unbindViewToModel: function () {
            if(this._options && this._options['changeTriggers']){
                _.each(this._options['changeTriggers'], function (event, selector) {
                    $(this._rootEl).undelegate(selector, event, this._onElChanged);
                }, this);
            }
        },

        _onElChanged:function (event) {
            var el, elBindings, elBindingCount, elBinding;

            el = $(event.target)[0];
            elBindings = this._getElBindings(el);

            for(elBindingCount = 0; elBindingCount < elBindings.length; elBindingCount++){
                elBinding = elBindings[elBindingCount];
                if (this._isBindingUserEditable(elBinding)) {
                    this._copyViewToModel(elBinding, el);
                }
            }
        },

        _isBindingUserEditable: function(elBinding){
            return elBinding.elAttribute === undefined ||
                elBinding.elAttribute === 'text' ||
                elBinding.elAttribute === 'html';
        },

        _isElUserEditable: function(el){
            var isContentEditable = el.attr('contenteditable');
            return isContentEditable || el.is('input') || el.is('select') || el.is('textarea');
        },

        _isBindingRadioGroup: function(elBinding){
            var elCount, el;
            var isAllRadioButtons = elBinding.boundEls.length > 0;
            for(elCount = 0; elCount < elBinding.boundEls.length; elCount++){
                el = $(elBinding.boundEls[elCount]);
                if(el.attr('type') !== 'radio'){
                    isAllRadioButtons = false;
                    break;
                }
            }

            return isAllRadioButtons;
        },

        _getRadioButtonGroupCheckedEl: function(elBinding){
            var elCount, el;
            for(elCount = 0; elCount < elBinding.boundEls.length; elCount++){
                el = $(elBinding.boundEls[elCount]);
                if(el.attr('type') === 'radio' && el.attr('checked')){
                    return el;
                }
            }

            return undefined;
        },

        _getElBindings:function (findEl) {
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

        _onModelChange:function () {
            var changedAttribute, attributeBinding;

            for (changedAttribute in this._model.changedAttributes()) {
                attributeBinding = this._attributeBindings[changedAttribute];

                if (attributeBinding) {
                    this._copyModelToView(attributeBinding);
                }
            }
        },

        _copyModelToView:function (attributeBinding) {
            var elementBindingCount, elementBinding, boundElCount, boundEl, value, convertedValue;

            value = this._model.get(attributeBinding.attributeName);

            for (elementBindingCount = 0; elementBindingCount < attributeBinding.elementBindings.length; elementBindingCount++) {
                elementBinding = attributeBinding.elementBindings[elementBindingCount];

                for (boundElCount = 0; boundElCount < elementBinding.boundEls.length; boundElCount++) {
                    boundEl = elementBinding.boundEls[boundElCount];

                    if(!boundEl._isSetting){
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

        _setElAttribute:function (el, elementBinding, convertedValue) {
            switch (elementBinding.elAttribute) {
                case 'html':
                    el.html(convertedValue);
                    break;
                case 'text':
                    el.text(convertedValue);
                    break;
                case 'enabled':
                    el.prop('disabled', !convertedValue);
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
                    var currentValue = this._model.get(elementBinding.attributeBinding.attributeName);
                    // is current value is now defined then remove the class the may have been set for the undefined value
                    if(!_.isUndefined(previousValue) || !_.isUndefined(currentValue)){
                        previousValue = this._getConvertedValue(Backbone.ModelBinder.Constants.ModelToView, elementBinding, previousValue);
                        el.removeClass(previousValue);
                    }

                    if(convertedValue){
                        el.addClass(convertedValue);
                    }
                    break;
                default:
                    el.attr(elementBinding.elAttribute, convertedValue);
            }
        },

        _setElValue:function (el, convertedValue) {
            if(el.attr('type')){
                switch (el.attr('type')) {
                    case 'radio':
                        if (el.val() === convertedValue) {
                            // must defer the change trigger or the change will actually fire with the old value
                            el.prop('checked') || _.defer(function() { el.trigger('change'); });
                            el.prop('checked', true);
                        }
                        else {
                            // must defer the change trigger or the change will actually fire with the old value
                            el.prop('checked', false);
                        }
                        break;
                    case 'checkbox':
                         // must defer the change trigger or the change will actually fire with the old value
                         el.prop('checked') === !!convertedValue || _.defer(function() { el.trigger('change') });
                         el.prop('checked', !!convertedValue);
                        break;
                    case 'file':
                        break;
                    default:
                        el.val(convertedValue);
                }
            }
            else if(el.is('input') || el.is('select') || el.is('textarea')){
                el.val(convertedValue || (convertedValue === 0 ? '0' : ''));
            }
            else {
                el.text(convertedValue || (convertedValue === 0 ? '0' : ''));
            }
        },

        _copyViewToModel: function (elementBinding, el) {
            var result, value, convertedValue;

            if (!el._isSetting) {

                el._isSetting = true;
                result = this._setModel(elementBinding, $(el));
                el._isSetting = false;

                if(result && elementBinding.converter){
                    value = this._model.get(elementBinding.attributeBinding.attributeName);
                    convertedValue = this._getConvertedValue(Backbone.ModelBinder.Constants.ModelToView, elementBinding, value);
                    this._setEl($(el), elementBinding, convertedValue);
                }
            }
        },

        _getElValue: function(elementBinding, el){
            switch (el.attr('type')) {
                case 'checkbox':
                    return el.prop('checked') ? true : false;
                default:
                    if(el.attr('contenteditable') !== undefined){
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
            return this._model.set(data,  this._options['modelSetOptions']);
        },

        _getConvertedValue: function (direction, elementBinding, value) {
            if (elementBinding.converter) {
                value = elementBinding.converter(direction, value, elementBinding.attributeBinding.attributeName, this._model, elementBinding.boundEls);
            }

            return value;
        },

        _throwException: function(message){
            if(this._options.suppressThrows){
                if(console && console.error){
                    console.error(message);
                }
            }
            else {
                throw message;
            }
        }
    });

    Backbone.ModelBinder.CollectionConverter = function(collection){
        this._collection = collection;

        if(!this._collection){
            throw 'Collection must be defined';
        }
        _.bindAll(this, 'convert');
    };

    _.extend(Backbone.ModelBinder.CollectionConverter.prototype, {
        convert: function(direction, value){
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
    Backbone.ModelBinder.createDefaultBindings = function(rootEl, attributeType, converter, elAttribute){
        var foundEls, elCount, foundEl, attributeName;
        var bindings = {};

        foundEls = $('[' + attributeType + ']', rootEl);

        for(elCount = 0; elCount < foundEls.length; elCount++){
            foundEl = foundEls[elCount];
            attributeName = $(foundEl).attr(attributeType);

            if(!bindings[attributeName]){
                var attributeBinding =  {selector: '[' + attributeType + '="' + attributeName + '"]'};
                bindings[attributeName] = attributeBinding;

                if(converter){
                    bindings[attributeName].converter = converter;
                }

                if(elAttribute){
                    bindings[attributeName].elAttribute = elAttribute;
                }
            }
        }

        return bindings;
    };

    // Helps you to combine 2 sets of bindings
    Backbone.ModelBinder.combineBindings = function(destination, source){
        _.each(source, function(value, key){
            var elementBinding = {selector: value.selector};

            if(value.converter){
                elementBinding.converter = value.converter;
            }

            if(value.elAttribute){
                elementBinding.elAttribute = value.elAttribute;
            }

            if(!destination[key]){
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