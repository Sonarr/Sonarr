'use strict';
define([
    'Settings/SettingsModelBase'], function (ModelBase) {
    return ModelBase.extend({

        baseInitialize: ModelBase.prototype.initialize,

        initialize: function () {
            var name = this.get('name');

            this.successMessage = 'Saved indexer: ' + name;
            this.errorMessage = 'Couldn\'t save indexer: ' + name;

            this.baseInitialize.call(this);
        }
    });
});
