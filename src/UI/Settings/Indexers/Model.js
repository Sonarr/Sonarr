'use strict';
define([
    'Settings/SettingsModelBase'], function (ModelBase) {
    return ModelBase.extend({

        baseInitialize: ModelBase.prototype.initialize,

        initialize: function () {
            var name = this.get('name');

            if (name) {
                this.successMessage = 'Saved indexer: ' + name;
                this.errorMessage = 'Couldn\'t save indexer: ' + name;
            }

            else {
                this.successMessage = 'Saved indexer';
                this.errorMessage = 'Couldn\'t save indexer';
            }

            this.baseInitialize.call(this);
        }
    });
});
