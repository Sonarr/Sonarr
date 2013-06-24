﻿'use strict';
define(['app',
    'Settings/SettingsModelBase'], function (App, SettingsModelBase) {
    return SettingsModelBase.extend({
        url           : window.ApiRoot + '/settings',
        successMessage: 'Settings saved',
        errorMessage  : 'Failed to save settings'
    });
});
