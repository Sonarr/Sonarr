define(['app'], function () {
    NzbDrone.HeaderView = Backbone.Marionette.ItemView.extend({
        events: {
            'click #logo': 'onClick'
        },

        onClick: function (event) {

            event.preventDefault();

            var target = $(event.target);
            var href = undefined;

            //look down for <a/>
            href = event.target.getAttribute('href');

            if (href && href.startsWith('http')) {
                window.location.href = href;
            } else {
                NzbDrone.Router.navigate(href, { trigger: true, replace: true });
            }

        },

        initialize: function () {
            this.setElement($('#in-nav'));
        }
    });

    return new NzbDrone.HeaderView();
});
