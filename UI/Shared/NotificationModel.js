define(['app'], function () {

    NzbDrone.Shared.NotificationModel = Backbone.Model.extend({
        mutators: {

            preFormattedMessage: function () {
                return  this.get('message').replace(/\\r\\n/g, '<br>');
            },

            isPreFormatted: function () {
                return this.get('message').indexOf('\\r\\n') !== -1;
            },

            iconClass: function () {

                if (this.has('icon')) {
                    return 'icon';
                }

                if (this.get('level') === 'info') {
                    return "icon-info-sign";
                } else if (this.get('level') === 'success') {
                    return 'icon-ok-sign';
                } else if (this.get('level') === 'error') {
                    return 'icon-warning-sign';
                }

                return "";
            }
        },

        defaults: {
            "level"  : 'info',
            "title"  : '',
            "message": ''
        }
    });
});