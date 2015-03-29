var $ = require('jquery');
var DeepModel = require('backbone.deepmodel');
var Messenger = require('../Shared/Messenger');

module.exports = DeepModel.extend({
    connectData : function(action) {
        var self = this;
        
        this.trigger('connect:sync');

        var promise = $.Deferred();

        var callAction = function(action) {
            var params = {};
            params.url = self.collection.url + '/connectData/' + action;
            params.contentType = 'application/json';
            params.data = JSON.stringify(self.toJSON());
            params.type = 'POST';
            params.isValidatedCall = true;

            $.ajax(params).fail(promise.reject).success(function(response) {
                if (response.action) 
                {

                    if (response.action === "openwindow") 
                    {
                        var connectResponseWindow = window.open(response.url);
                        var selfWindow = window;
                        selfWindow.onCompleteOauth = function(query, callback) {
                            delete selfWindow.onCompleteOauth;
                            if (response.nextStep) { callAction(response.nextStep + query); }
                            else { promise.resolve(response); }
                            callback();
                        };
                        return;
                    } 
                    else if (response.action === "updatefields")
                    {
                        Object.keys(response.fields).forEach(function(field) {
                            self.set(field, response.fields[field]);
                            self.attributes.fields.forEach(function(fieldDef) {
                                if (fieldDef.name === field) { fieldDef.value = response.fields[field]; }
                            });
                        });
                    }
                }
                if (response.nextStep) { callAction(response.nextStep); }
                else { promise.resolve(response); }
            });
        };

        callAction(action);

        Messenger.monitor({
            promise        : promise,
            successMessage : 'Connecting for \'{0}\' completed'.format(this.get('name')),
            errorMessage   : 'Connecting for \'{0}\' failed'.format(this.get('name'))
        });

        promise.fail(function(response) {
            self.trigger('connect:failed', response);
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
            successMessage : 'Testing \'{0}\' completed'.format(this.get('name')),
            errorMessage   : 'Testing \'{0}\' failed'.format(this.get('name'))
        });

        promise.fail(function(response) {
            self.trigger('validation:failed', response);
        });

        return promise;
    }
});