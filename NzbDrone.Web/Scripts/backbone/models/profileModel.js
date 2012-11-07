window.Profile = Backbone.Model.extend({

    urlRoot: '/api/qualityprofiles',

    idAttribute: 'id',

    initialize: function () {
        this.validators = {};

        this.validators.name = function (value) {
            return value.length > 0 ? { isValid: true } : { isValid: false, message: 'You must enter a name' };
        };

        this.validators.allowed = function (value) {
            return value.length > 0 ? { isValid: true } : { isValid: false, message: 'You must have allowed qualities' };
        };
        
        this.validators.cutoff = function (value) {
            return value != null ? { isValid: true } : { isValid: false, message: 'You must have a valid cutoff' };
        };
    },

    validateItem: function (key) {
        return (this.validators[key]) ? this.validators[key](this.get(key)) : { isValid: true };
    },

    // TODO: Implement Backbone's standard validate() method instead.
    validateAll: function () {

        var messages = {};

        for (var key in this.validators) {
            if (this.validators.hasOwnProperty(key)) {
                var check = this.validators[key](this.get(key));
                if (check.isValid === false) {
                    messages[key] = check.message;
                }
            }
        }

        return _.size(messages) > 0 ? { isValid: false, messages: messages } : { isValid: true };
    },

    defaults: {
        id: null,
        name: '',
        allowed: {},
        cutoff: null
    }
});