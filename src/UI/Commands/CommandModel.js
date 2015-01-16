var _ = require('underscore');
var Backbone = require('backbone');

module.exports = Backbone.Model.extend({
    url           : window.NzbDrone.ApiRoot + '/command',
    parse         : function(response){
        response.name = response.name.toLocaleLowerCase();

                for (var key in response.body) {
                    response[key] = response.body[key];
                }

                delete response.body;

        return response;
    },
    isSameCommand : function(command){
        if(command.name.toLocaleLowerCase() !== this.get('name').toLocaleLowerCase()) {
            return false;
        }
        for (var key in command) {
            if(key !== 'name') {
                if(Array.isArray(command[key])) {
                    if(_.difference(command[key], this.get(key)).length > 0) {
                        return false;
                    }
                }
                else if(command[key] !== this.get(key)) {
                    return false;
                }
            }
        }
        return true;
    },
    isActive      : function(){
        return this.get('state') !== 'completed' && this.get('state') !== 'failed';
    },
    isComplete    : function(){
        return this.get('state') === 'completed';
    }
});