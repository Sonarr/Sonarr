'use strict';
define(
    [
        'Settings/SettingsModelBase'
    ], function (SettingsModelBase) {
        return SettingsModelBase.extend({
            url           : window.NzbDrone.ApiRoot + '/config/indexer',
            successMessage: 'Indexer settings saved',
            errorMessage  : 'Failed to save indexer settings'
        });
    });
