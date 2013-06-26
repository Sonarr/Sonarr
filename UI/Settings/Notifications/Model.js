'use strict';
define([
    'Settings/SettingsModelBase'], function (ModelBase) {
    return ModelBase.extend({

        successMessage: 'Notification Saved',
        errorMessage  : 'Couldn\'t save notification'
    });
});
