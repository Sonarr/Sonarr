function grid_bind(args) {
    var id = this.attributes[0].textContent;
    var parent = $('#' + id).parent();
    parent.children('.grid-loader').stop().css("top", "0px").fadeIn('slow');
}

function grid_bound(args) {
    var id = this.attributes[0].textContent;
    var parent = $('#' + id).parent();
    $('.grid-container').children('.grid-loader').stop().fadeOut('slow');
}