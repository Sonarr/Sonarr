
(function() {

var showingNav = true;

$(document).ready( function () {
	var jqNav = $('div.fw_nav');
	jqNav.css('right', ($(window).width() - $('div.fw_container').width()) /2);

	var n = $('div.nav_blocker')[0];
	n.style.height = $(jqNav).outerHeight()+"px";
	n.style.width = $(jqNav).outerWidth()+"px";

	SyntaxHighlighter.highlight();

	$('#private_toggle').click( function () {
		if ( $('input[name=show_private]').val() == 0 ) {
			$('input[name=show_private]').val( 1 );
			$('#private_label').html('Showing');
			$('.private').css('display', 'block');
		} else {
			$('input[name=show_private]').val( 0 );
			$('#private_label').html('Hiding');
			$('.private').css('display', 'none');
		}
		fnWriteCookie();
		return false;
	} );

	$('#extended_toggle').click( function () {
		if ( $('input[name=show_extended]').val() == 0 ) {
			$('input[name=show_extended]').val( 1 );
			$('#extended_label').html('Showing');
			$('.augmented').css('display', 'block');
		} else {
			$('input[name=show_extended]').val( 0 );
			$('#extended_label').html('Hiding');
			$('.augmented').css('display', 'none');
		}
		fnWriteCookie();
		return false;
	} );

	var savedHeight = $(jqNav).height();
	$('div.fw_nav h2').click( function () {
		if ( showingNav ) {
			$('div.fw_nav').animate( {
				"height": 10,
				"opacity": 0.3
			} );
			showingNav = false;
		} else {
			$('div.fw_nav').animate( {
				"height": savedHeight,
				"opacity": 1
			} );
			showingNav = true;
		}
		fnWriteCookie();
	} );

	var cookie = fnReadCookie( 'SpryMedia_JSDoc' );
	if ( cookie != null ) {
		var a = cookie.split('-');
		if ( a[0] == 1 ) {
			$('#private_toggle').click();
		}
		if ( a[1] == 0 ) {
			$('#extended_toggle').click();
		}
		if ( a[2] == 'false' ) {
			$('div.fw_nav').css('height', 10).css('opacity', 0.3);
			showingNav = false;
		}
	}
} );


function fnWriteCookie()
{
	var sVal = 
		$('input[name=show_private]').val()+'-'+
		$('input[name=show_extended]').val()+'-'+
		showingNav;
	
	fnCreateCookie( 'SpryMedia_JSDoc', sVal );
}


function fnCreateCookie( sName, sValue )
{
	var iDays = 365;
	var date = new Date();
	date.setTime( date.getTime()+(iDays*24*60*60*1000) );
	var sExpires = "; expires="+date.toGMTString();
	
	document.cookie = sName+"="+sValue+sExpires+"; path=/";
}


function fnReadCookie( sName )
{
	var sNameEQ = sName + "=";
	var sCookieContents = document.cookie.split(';');
	
	for( var i=0 ; i<sCookieContents.length ; i++ ) {
		var c = sCookieContents[i];
		
		while (c.charAt(0)==' ') {
			c = c.substring(1,c.length);
		}
		
		if (c.indexOf(sNameEQ) == 0) {
			return c.substring(sNameEQ.length,c.length);
		}
	}
	
	return null;
}

})();