'use strict';

define(['app','Cells/NzbDroneCell'], function () {
    NzbDrone.Cells.TemplatedCell = NzbDrone.Cells.NzbDroneCell.extend({


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
