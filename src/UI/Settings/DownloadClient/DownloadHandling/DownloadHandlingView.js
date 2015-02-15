var Marionette = require('marionette');
var AsModelBoundView = require('../../../Mixins/AsModelBoundView');
var AsValidatedView = require('../../../Mixins/AsValidatedView');

var view = Marionette.ItemView.extend({
    template : 'Settings/DownloadClient/DownloadHandling/DownloadHandlingViewTemplate',

    ui : {
        completedDownloadHandlingCheckbox : '.x-completed-download-handling',
        completedDownloadOptions          : '.x-completed-download-options',
        failedAutoRedownladCheckbox       : '.x-failed-auto-redownload',
        failedDownloadOptions             : '.x-failed-download-options'
    },

    events : {
        'change .x-completed-download-handling' : '_setCompletedDownloadOptionsVisibility',
        'change .x-failed-auto-redownload'      : '_setFailedDownloadOptionsVisibility'
    },

    onRender : function() {
        if (!this.ui.completedDownloadHandlingCheckbox.prop('checked')) {
            this.ui.completedDownloadOptions.hide();
        }
        if (!this.ui.failedAutoRedownladCheckbox.prop('checked')) {
            this.ui.failedDownloadOptions.hide();
        }
    },

    _setCompletedDownloadOptionsVisibility : function() {
        var checked = this.ui.completedDownloadHandlingCheckbox.prop('checked');
        if (checked) {
            this.ui.completedDownloadOptions.slideDown();
        } else {
            this.ui.completedDownloadOptions.slideUp();
        }
    },

    _setFailedDownloadOptionsVisibility : function() {
        var checked = this.ui.failedAutoRedownladCheckbox.prop('checked');
        if (checked) {
            this.ui.failedDownloadOptions.slideDown();
        } else {
            this.ui.failedDownloadOptions.slideUp();
        }
    }
});

AsModelBoundView.call(view);
AsValidatedView.call(view);
module.exports = view;