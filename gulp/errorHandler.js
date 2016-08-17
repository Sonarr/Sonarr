module.exports = {
    onError : function(error) {
        //If you want details of the error in the console
        console.log(error.toString());
        this.emit('end');
    }
};