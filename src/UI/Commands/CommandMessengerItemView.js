var Marionette = require('marionette');
var Messenger = require('../Shared/Messenger');

module.exports = Marionette.ItemView.extend({
    initialize : function() {
        this.listenTo(this.model, 'change', this.render);
    },

    render : function() {
        if (!this.model.get('message') || !this.model.get('sendUpdatesToClient')) {
            return;
        }

        var message = {
            type      : 'info',
            message   : '[{0}] {1}'.format(this.model.get('name'), this.model.get('message')),
            id        : this.model.id,
            hideAfter : 0
        };

        var isManual = this.model.get('manual');

        switch (this.model.get('state')) {
            case 'completed':
                message.hideAfter = 4;
                break;
            case 'failed':
                message.hideAfter = isManual ? 10 : 4;
                message.type = 'error';
                break;
            default :
                message.hideAfter = 0;
        }

        if (this.messenger) {
            this.messenger.update(message);
        }

        else {
            this.messenger = Messenger.show(message);
        }

        console.log(message.message);
    }
});