'use strict';

define(
    [
        'Settings/SettingsModelBase'
    ], function (ModelBase) {
        return ModelBase.extend({

            baseInitialize: ModelBase.prototype.initialize,

            initialize: function () {
                var name = this.get('quality').name;

                this.successMessage = 'Saved ' + name + ' quality settings';
                this.errorMessage = 'Couldn\'t save ' + name + ' quality settings';

                this.baseInitialize.call(this);
            }
        });
    });
