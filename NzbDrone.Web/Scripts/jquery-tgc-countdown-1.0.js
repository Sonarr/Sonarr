/* The Great Cornholio Countdown, version: 1.00 (October 14th, 2010)
 * Copyright (c) 2010 Lecho Buszczynski
 * lecho@phatcat.eu
 * Licensed under the MIT (http://www.opensource.org/licenses/mit-license.php)
 * Requires: jQuery v?.?
 */
 
(function($) {

	$.fn.tgcCountdown = function(options) {

		var settings	= {counter: '[D] days, [H]:[M]:[S]',counter_warning: '<span style="color: #F00;">[D] days, [H]:[M]:[S]</span>',counter_expired: '<span style="color: #F00;">expired</span>', interval: 1000, warnonminutesleft: 60};
		
		$.extend(settings,options);

		var target		= $(this);
		var stamp		= $(this).html();
		var date_e		= false;
		var executer	= false;
		
		var getSecondsDiff = function() {
			
			var secs	= 0;
			var now		= new Date();
			
			secs = date_e.getTime() - now.getTime();

			return (secs/1000);
		}
		
		var getFormattedDiff = function() {
		
			var secs	= getSecondsDiff();
			var counter	= '';
			var val		= '';
			
			if (secs < 0) {
				counterStop();
				return settings.counter_expired;
			} else if (secs < settings.warnonminutesleft*60) {
				counter = settings.counter_warning;
			} else {
				counter = settings.counter;
			}
			
			val = new String(Math.floor(secs/86400));
			
			counter = counter.replace('[D]',val);			

			secs = secs % 86400;
			
			val = new String(Math.floor(secs/3600));
			
			if (val.length == 1) {
				val = '0' + val;
			}
			
			counter = counter.replace('[H]',val);
			
			secs = secs % 3600;

			val = new String(Math.floor(secs/60));

			if (val.length == 1) {
				val = '0' + val;
			}
			
			counter = counter.replace('[M]',val);

			secs = secs % 60;
			
			val = new String(Math.floor(secs));
			
			if (val.length == 1) {
				val = '0' + val;
			}
			
			counter = counter.replace('[S]',val);
			return counter;
		}
		
		var counterUpdate = function() {
			target.html(getFormattedDiff());
		}
		
		var counterStart = function() {
			
			counterUpdate();
			executer = setInterval(function() {counterUpdate()},settings.interval);

		}
		
		var counterStop = function() {
			clearInterval(executer);
		}
		
		if (/^[0-9]{14}$/.test(stamp)) {
			
			date_e = new Date(stamp.substr(0,4),(stamp.substr(4,2)- 1),stamp.substr(6,2),stamp.substr(8,2),stamp.substr(10,2),stamp.substr(12,2),0);

			counterStart()
		}

	};

})(jQuery);