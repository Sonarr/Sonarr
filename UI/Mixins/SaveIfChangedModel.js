"use strict";
(function () {

    NzbDrone.Mixins.SaveIfChangedModel = {
//        originalInitialize: this.initialize,

        initialize: function () {
            this.isSaved = true;

            this.on('change', function () {
                this.isSaved = false;
            }, this);

            this.on('sync', function () {
                this.isSaved = true;
            }, this);

//            if (originalInitialize) {
//                originalInitialize.call(this);
//            }
        },

        saveIfChanged: function (options) {
            if (!this.isSaved) {
                this.save(undefined, options);
            }
        }
    };
}());