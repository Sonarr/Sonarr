"use strict";

define(
    [
        'Settings/SettingsModelBase'
    ], function (ModelBase) {
        return ModelBase.extend({

            baseInitialize: ModelBase.prototype.initialize,

            initialize: function () {
                var name = this.get('name');

                this.successMessage = 'Saved ' + name + ' size settings';
                this.errorMessage = 'Couldn\'t save ' + name + ' size settings';

                this.baseInitialize.call(this);
            }
        });
    });
