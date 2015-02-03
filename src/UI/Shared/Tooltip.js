var $ = require('jquery');

module.exports = {
    appInitializer : function(){
        console.log('starting signalR');
        $('body').tooltip({selector : '[title]'});
        return this;
    }
};