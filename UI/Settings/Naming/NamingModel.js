﻿'use strict';
define(
    [
        'Settings/SettingsModelBase'
    ], function (ModelBase) {
        return  ModelBase.extend({
            url           : window.ApiRoot + '/config/naming',
            successMessage: 'Naming settings saved',
            errorMessage  : 'Couldn\'t save naming settings'
        });

    });
