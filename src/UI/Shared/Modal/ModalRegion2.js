var $ = require('jquery');
var ModalRegionBase = require('./ModalRegionBase');

var region = ModalRegionBase.extend({
    el : '#modal-region2',

    initialize : function () {
        this.listenTo(this, 'modal:beforeShow', this.onBeforeShow);
    },

    onBeforeShow : function () {
        this.$el.addClass('modal fade');
        this.$el.attr('tabindex', '-1');
        this.$el.css('z-index', '1060');

        this.$el.on('shown.bs.modal', function() {
            $('.modal-backdrop:last').css('z-index', 1059);
        });
    },

    _closed : function () {
        ModalRegionBase.prototype._closed.apply(this, arguments);

        if (require('../../AppLayout').modalRegion.currentView) {
            $('body').addClass('modal-open');
        }
    }
});

module.exports = region;