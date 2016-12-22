var DeepModel = require('backbone.deepmodel');

module.exports = DeepModel.extend({
    defaults : {
        id     : null,
        name   : '',
        cutoff : null
    }
});