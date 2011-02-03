/*
Name: jQuery Simple Drop Down Plugin
Author: Etienne Fardet
Version: 1.2
*/

(function($){  
 $.simpledropdown = function(selector) {
	 
		$(selector).children("ul").addClass("dropdown");
		$("ul.dropdown>li:first-child").addClass("selected");
		$("ul.dropdown>li").not(".dropdown>li:first-child").addClass("drop");		

		$("ul.dropdown").click(function() {
			var subitems = $(this).find(".drop ul li");
			var selecteditem = $(this).find(".selected");
			subitems.slideToggle("fast", function() {
				
			subitems.click(function() {
				var selection = $(this).text();
				selecteditem.text(selection).fadeOut(5, function() {
					if (jQuery.browser.msie) {
						$(this).fadeIn(100);
					} else {
						$(this).fadeIn(400);
					}
				});
			});
		});
	});
 };  
})(jQuery); 
