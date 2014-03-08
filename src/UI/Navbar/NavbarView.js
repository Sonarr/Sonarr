'use strict';
define(
    [
        'marionette',
        'jquery',
        'Health/HealthView',
        'Navbar/Search'
    ], function (Marionette, $, HealthView) {
        return Marionette.Layout.extend({
            template: 'Navbar/NavbarLayoutTemplate',

            regions: {
                health: '#x-health'
            },

            ui: {
                search: '.x-series-search'
            },

            events: {
                'click a': 'onClick'
            },

            onRender: function () {
                this.ui.search.bindSearch();
                this.health.show(new HealthView());
            },

            onClick: function (event) {

                event.preventDefault();

                var target = $(event.target);

                //look down for <a/>
                var href = event.target.getAttribute('href');

                //if couldn't find it look up'
                if (!href && target.closest('a') && target.closest('a')[0]) {

                    var linkElement = target.closest('a')[0];

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
