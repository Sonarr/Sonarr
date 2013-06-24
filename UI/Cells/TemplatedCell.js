'use strict';

define(
    [
        'marionette',
        'Cells/NzbDroneCell'
    ], function (Marionette, NzbDroneCell) {
        return NzbDroneCell.extend({


            render: function () {

                var templateName = this.column.get('template') || this.template;

                this.templateFunction = Marionette.TemplateCache.get(templateName);
                var data = this.cellValue.toJSON();
                var html = this.templateFunction(data);
                this.$el.html(html);

                return this;
            }
        });
    });
