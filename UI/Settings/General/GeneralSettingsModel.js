﻿'use strict';
define(
    [
        'Settings/SettingsModelBase'
    ], function (SettingsModelBase) {
        return SettingsModelBase.extend({

            url           : window.ApiRoot + '/settings/host',
            successMessage: 'General settings saved',
            errorMessage  : 'Failed to save general settings'

        });
    });
