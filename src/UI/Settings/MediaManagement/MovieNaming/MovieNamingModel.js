var ModelBase = require('../../SettingsModelBase');

module.exports = ModelBase.extend({
    url            : window.NzbDrone.ApiRoot + '/config/naming',
    successMessage : 'MediaManagement settings saved',
    errorMessage   : 'Couldn\'t save naming settings'
});