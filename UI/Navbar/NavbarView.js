'use strict';
define(
    [
        'marionette'
    ], function (Marionette) {
        return Marionette.ItemView.extend({
            events: {
                'click a': 'onClick'
            },

            template : 'Navbar/NavbarTemplate',

            onClick: function (event) {

                event.preventDefault();

                var target = $(event.target);

                //look down for <a/>
                var href = event.target.getAttribute('href');

                //if couldn't find it look up
                if (!href && target.parent('a') && target.parent('a')[0]) {

                    var linkElement = target.parent('a')[0];

                    href = linkElement.getAttribute('href');
                    this.setActive(linkElement);
                }
                else {
                    this.setActive(event.target);
                }
            },

            setActive: function (element) {
                //Todo: Set active on first load
                this.$('a').removeClass('active');
                $(element).addClass('active');
            }
        });
    });
