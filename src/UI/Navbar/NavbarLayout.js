var Marionette = require('marionette');
var $ = require('jquery');
var HealthView = require('../Health/HealthView');
var QueueView = require('../Activity/Queue/QueueView');
require('./Search');

module.exports = Marionette.Layout.extend({
    template : 'Navbar/NavbarLayoutTemplate',

    regions : {
        health : '#x-health',
        queue  : '#x-queue-count'
    },

    ui : {
        search   : '.x-series-search',
        collapse : '.x-navbar-collapse'
    },

    events : {
        'click a' : 'onClick'
    },

    onRender : function() {
        this.ui.search.bindSearch();
        this.health.show(new HealthView());
        this.queue.show(new QueueView());
    },

    onClick : function(event) {
        var target = $(event.target);

        //look down for <a/>
        var href = event.target.getAttribute('href');

        if (href && href.startsWith("http")) {
            return;
        }

        event.preventDefault();

        //if couldn't find it look up'
        if (!href && target.closest('a') && target.closest('a')[0]) {

            var linkElement = target.closest('a')[0];

            href = linkElement.getAttribute('href');
            this.setActive(linkElement);
        } else {
            this.setActive(event.target);
        }

        if ($(window).width() < 768) {
            this.ui.collapse.collapse('hide');
        }
    },

    setActive : function(element) {
        //Todo: Set active on first load
        this.$('a').removeClass('active');
        $(element).addClass('active');
    }
});