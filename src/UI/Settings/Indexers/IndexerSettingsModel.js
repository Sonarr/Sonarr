var SettingsModelBase = require('../SettingsModelBase');

module.exports = SettingsModelBase.extend({
    url            : window.NzbDrone.ApiRoot + '/config/indexer',
    successMessage : 'Indexer settings saved',
    errorMessage   : 'Failed to save indexer settings'
});