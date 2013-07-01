"use strict";

define(
    [
        'app',
        'Settings/SettingsModelBase'
    ], function (App, ModelBase) {
        return ModelBase.extend({

            baseInitialize: ModelBase.prototype.initialize,

            initialize: function () {
                var name = this.get('name');

                this.successMessage = 'Saved ' + name + ' size settings';
                this.errorMessage = 'Couldn\'t save ' + name + ' size settings';

                this.baseInitialize.call(this);
            },

            mutators: {
                thirtyMinuteSize: function () {
                    var maxSize = this.get('maxSize');

                    if (maxSize === 0) {
                        return 'No Limit';
                    }

                    return (maxSize * 1024 * 1024 * 30).bytes(1);
                },
                sixtyMinuteSize : function () {
                    var maxSize = this.get('maxSize');

                    if (maxSize === 0) {
                        return 'No Limit';
                    }

                    return (maxSize * 1024 * 1024 * 60).bytes(1);
                }
            }
        });
    });
