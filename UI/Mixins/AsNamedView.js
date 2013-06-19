'use strict';

define(
    function () {

        return function () {

            this.viewName = function () {
                if (this.template) {
                    var regex = new RegExp('\/', 'g');

                    return this.template
                        .toLocaleLowerCase()
                        .replace('template', '')
                        .replace(regex, '-');
                }

                return undefined;
            };

            var originalOnRender = this.onRender;

            this.onRender = function () {

                this.$el.removeClass('iv-' + this.viewName());
                this.$el.addClass('iv-' + this.viewName());

                if (originalOnRender) {
                    return   originalOnRender.call(this);
                }

                return undefined;
            };

            return this;
        };
    }
);
