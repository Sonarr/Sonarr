$.fn.dataTableExt.oApi.fnSetFilteringDelay = function (oSettings, iDelay) {
	/*
	* Inputs:      object:oSettings - dataTables settings object - automatically given
	*              integer:iDelay - delay in milliseconds
	* Usage:       $('#example').dataTable().fnSetFilteringDelay(250);
	* Author:      Zygimantas Berziunas (www.zygimantas.com) and Allan Jardine
	* License:     GPL v2 or BSD 3 point style
	* Contact:     zygimantas.berziunas /AT\ hotmail.com
	*/
	var _that = this,
		iDelay = (typeof iDelay == 'undefined') ? 250 : iDelay;

	this.each(function (i) {
		$.fn.dataTableExt.iApiIndex = i;
		var oTimerId = null,
			sPreviousSearch = null,
			anControl = $('input', _that.fnSettings().aanFeatures.f);

		anControl.unbind('keyup').bind('keyup', function () {
			if (sPreviousSearch === null || sPreviousSearch != anControl.val()) {
				window.clearTimeout(oTimerId);
				sPreviousSearch = anControl.val();
				oTimerId = window.setTimeout(function () {
					$.fn.dataTableExt.iApiIndex = i;
					_that.fnFilter(anControl.val());
				}, iDelay);
			}
		});

		return this;
	});
	return this;
};