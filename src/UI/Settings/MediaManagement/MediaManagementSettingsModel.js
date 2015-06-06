var SettingsModelBase = require('../SettingsModelBase');

module.exports = SettingsModelBase.extend({
    url            : window.NzbDrone.ApiRoot + '/config/mediamanagement',
    successMessage : 'Media management settings saved',
    errorMessage   : 'Failed to save media management settings'
});