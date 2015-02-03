var $ = require('jquery');

module.exports = {
    appInitializer : function(){
        $('body').tooltip({selector : '[title]'});
        return this;
    }
};