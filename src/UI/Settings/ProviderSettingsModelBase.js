var $ = require('jquery');
var DeepModel = require('backbone.deepmodel');
var Messenger = require('../Shared/Messenger');

module.exports = DeepModel.DeepModel.extend({
	connectData : function() {
		var self = this;
		
		this.trigger('connect:sync');

		var params = {};

		params.url = this.collection.url + '/connectData/step1';
		params.contentType = 'application/json';
        params.data = JSON.stringify(this.toJSON());
        params.type = 'POST';
        params.isValidatedCall = true;

        var promise = $.Deferred();
		$.ajax(params).fail(promise.reject).success(function(response) {
            if (response.redirectURL) {
				var connectResponseWindow = window.open(response.redirectURL);
                var selfWindow = window;
                selfWindow.onCompleteOauth = function(query, callback) {
                    delete selfWindow.onCompleteOauth;
                    params.url = self.collection.url + '/connectData/step2' + query;

                    $.ajax(params).fail(promise.reject).success(function(response) {
                        promise.resolve(response);
                    });

                    callback();

                };
            }
		});

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