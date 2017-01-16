var ModelBase = require('../Settings/SettingsModelBase');

module.exports = ModelBase.extend({
    baseInitialize : ModelBase.prototype.initialize,

    initialize : function() {
        var name = this.get('quality').name;

        this.successMessage = 'Saved ' + name + ' quality settings';
        this.errorMessage = 'Couldn\'t save ' + name + ' quality settings';

        this.baseInitialize.call(this);
    }
});