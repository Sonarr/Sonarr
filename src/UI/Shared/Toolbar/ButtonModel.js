var _ = require('underscore');
var Backbone = require('backbone');

module.exports = Backbone.Model.extend({
    defaults : {
        'target'  : '/nzbdrone/route',
        'title'   : '',
        'active'  : false,
        'tooltip' : undefined
    }
});