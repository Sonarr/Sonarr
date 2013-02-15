define(['app'], function () {

    NzbDrone.Shared.NotificationModel = Backbone.Model.extend({
        mutators:{
            pre:function () {
                try {
                    if (this.get('message')) {
                        return this.get('message').lines().lenght > 1;
                    }
                } catch (error) {
                    return false;
                }

            },
            iconClass:function () {

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

        defaults:{
            "level":'info',
            "title":'',
            "message":''
        }
    });
});