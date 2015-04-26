module.exports = function() {
    var originalInit = this.prototype.initialize;

    this.prototype.initialize = function() {

        this.isSaved = true;

        this.on('change', function() {
            this.isSaved = false;
        }, this);

        this.on('sync', function() {
            this.isSaved = true;
        }, this);

        if (originalInit) {
            originalInit.call(this);
        }
    };

    return this;
};