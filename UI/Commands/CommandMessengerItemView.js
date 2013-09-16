'use strict';
define(
    [
        'app',
        'marionette',
        'Shared/Messenger'
    ], function (App, Marionette, Messenger) {

        return Marionette.ItemView.extend({


            initialize: function () {
                this.listenTo(this.model, 'change', this.render);
            },


            render: function () {
                if (!this.model.get('message') || !this.model.get('sendUpdatesToClient')) {
                    return;
                }

                var message = {
                    type     : 'info',
                    message  : '[{0}] {1}'.format(this.model.get('name'), this.model.get('message')),
                    id       : this.model.id,
                    hideAfter: 0
                };

                switch (this.model.get('state')) {
                    case 'completed':
                        message.hideAfter = 4;
                        break;
                    case 'failed':
                        message.hideAfter = 4;
                        message.type = 'error';
                        break;
                    default :
                        message.hideAfter = 0;
                }

                Messenger.show(message);

                console.log(message.message);
            }

        });

    });
