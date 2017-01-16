var $ = require('jquery');
var _ = require('underscore');
var DeepModel = require('backbone.deepmodel');
var Messenger = require('../Shared/Messenger');

module.exports = DeepModel.extend({

    getFieldValue : function(name) {
        var index = _.indexOf(_.pluck(this.get('fields'), 'name'), name);
        return this.get('fields.' + index + '.value');
    },

    setFieldValue : function(name, value) {
        var index = _.indexOf(_.pluck(this.get('fields'), 'name'), name);
        return this.set('fields.' + index + '.value', value);
    },

    requestAction : function(action, queryParams) {
        var self = this;

        this.trigger('validation:sync');

        var params = {
            url             : this.collection.url + '/action/' + action,
            contentType     : 'application/json',
            data            : JSON.stringify(this.toJSON()),
            type            : 'POST',
            isValidatedCall : true
        };

        if (queryParams) {
            params.url += '?' + $.param(queryParams, true);
        }

        var promise = $.ajax(params);

        promise.fail(function(response) {
            self.trigger('validation:failed', response);
        });

        return promise;
    },

    test : function() {
        var self = this;

        this.trigger('validation:sync');

        var params = {};

        params.url = this.collection.url + '/test';
        params.contentType = 'application/json';
        params.data = JSON.stringify(this.toJSON());
        params.type = 'POST';
        params.isValidatedCall = true;

        var promise = $.ajax(params);

        Messenger.monitor({
            promise        : promise,
            successMessage : 'Testing \'{0}\' succeeded'.format(this.get('name')),
            errorMessage   : 'Testing \'{0}\' failed'.format(this.get('name'))
        });

        promise.fail(function(response) {
            self.trigger('validation:failed', response);
        });

        return promise;
    }
});