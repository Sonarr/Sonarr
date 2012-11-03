/**
 * @summary     Scroller
 * @description Virtual rendering for DataTables
 * @file        Scroller.js
 * @version     1.1.0
 * @author      Allan Jardine (www.sprymedia.co.uk)
 * @license     GPL v2 or BSD 3 point style
 * @contact     www.sprymedia.co.uk/contact
 *
 * @copyright Copyright 2011-2012 Allan Jardine, all rights reserved.
 *
 * This source file is free software, under either the GPL v2 license or a
 * BSD style license, available at:
 *   http://datatables.net/license_gpl2
 *   http://datatables.net/license_bsd
 */

(/** @lends <global> */function($, window, document) {


/** 
 * Scroller is a virtual rendering plug-in for DataTables which allows large
 * datasets to be drawn on screen every quickly. What the virtual rendering means
 * is that only the visible portion of the table (and a bit to either side to make
 * the scrolling smooth) is drawn, while the scrolling container gives the 
 * visual impression that the whole table is visible. This is done by making use
 * of the pagination abilities of DataTables and moving the table around in the
 * scrolling container DataTables adds to the page. The scrolling container is
 * forced to the height it would be for the full table display using an extra 
 * element. 
 * 
 * Note that rows in the table MUST all be the same height. Information in a cell
 * which expands on to multiple lines will cause some odd behaviour in the scrolling.
 *
 * Scroller is initialised by simply including the letter 'S' in the sDom for the
 * table you want to have this feature enabled on. Note that the 'S' must come
 * AFTER the 't' parameter in sDom.
 * 
 * Key features include:
 *   <ul class="limit_length">
 *     <li>Speed! The aim of Scroller for DataTables is to make rendering large data sets fast</li>
 *     <li>Full compatibility with deferred rendering in DataTables 1.9 for maximum speed</li>
 *     <li>Correct visual scrolling implementation, similar to "infinite scrolling" in DataTable core</li>
 *     <li>Integration with state saving in DataTables (scrolling position is saved)</li>
 *     <li>Easy to use</li>
 *   </ul>
 *
 *  @class
 *  @constructor
 *  @param {object} oDT DataTables settings object
 *  @param {object} [oOpts={}] Configuration object for FixedColumns. Options are defined by {@link Scroller.oDefaults}
 * 
 *  @requires jQuery 1.4+
 *  @requires DataTables 1.9.0+
 * 
 *  @example
 * 		$(document).ready(function() {
 * 			$('#example').dataTable( {
 * 				"sScrollY": "200px",
 * 				"sAjaxSource": "media/dataset/large.txt",
 * 				"sDom": "frtiS",
 * 				"bDeferRender": true
 * 			} );
 * 		} );
 */
var Scroller = function ( oDTSettings, oOpts ) {
	/* Sanity check - you just know it will happen */
	if ( ! this instanceof Scroller )
	{
		alert( "Scroller warning: Scroller must be initialised with the 'new' keyword." );
		return;
	}
	
	if ( typeof oOpts == 'undefined' )
	{
		oOpts = {};
	}
	
	/**
	 * Settings object which contains customisable information for the Scroller instance
	 * @namespace
	 * @extends Scroller.DEFAULTS
	 */
	this.s = {
		/** 
		 * DataTables settings object
		 *  @type     object
		 *  @default  Passed in as first parameter to constructor
		 */
		"dt": oDTSettings,
		
		/** 
		 * Pixel location of the top of the drawn table in the viewport
		 *  @type     int
		 *  @default  0
		 */
		"tableTop": 0,
		
		/** 
		 * Pixel location of the bottom of the drawn table in the viewport
		 *  @type     int
		 *  @default  0
		 */
		"tableBottom": 0,
		
		/** 
		 * Pixel location of the boundary for when the next data set should be loaded and drawn
		 * when scrolling up the way.
		 *  @type     int
		 *  @default  0
		 *  @private
		 */
		"redrawTop": 0,
		
		/** 
		 * Pixel location of the boundary for when the next data set should be loaded and drawn
		 * when scrolling down the way. Note that this is actually caluated as the offset from
		 * the top.
		 *  @type     int
		 *  @default  0
		 *  @private
		 */
		"redrawBottom": 0,
		
		/** 
		 * Height of rows in the table
		 *  @type     int
		 *  @default  0
		 */
		"rowHeight": null,
		
		/** 
		 * Auto row height or not indicator
		 *  @type     bool
		 *  @default  0
		 */
		"autoHeight": true,
		
		/** 
		 * Pixel height of the viewport
		 *  @type     int
		 *  @default  0
		 */
		"viewportHeight": 0,
		
		/** 
		 * Number of rows calculated as visible in the visible viewport
		 *  @type     int
		 *  @default  0
		 */
		"viewportRows": 0,
		
		/** 
		 * setTimeout reference for state saving, used when state saving is enabled in the DataTable
		 * and when the user scrolls the viewport in order to stop the cookie set taking too much
		 * CPU!
		 *  @type     int
		 *  @default  0
		 */
		"stateTO": null,
		
		/** 
		 * setTimeout reference for the redraw, used when server-side processing is enabled in the
		 * DataTables in order to prevent DoSing the server
		 *  @type     int
		 *  @default  null
		 */
		"drawTO": null
	};
	this.s = $.extend( this.s, Scroller.oDefaults, oOpts );
	
	/**
	 * DOM elements used by the class instance
	 * @namespace
	 * 
	 */
	this.dom = {
		"force": document.createElement('div'),
		"scroller": null,
		"table": null
	};

	/* Attach the instance to the DataTables instance so it can be accessed */
	this.s.dt.oScroller = this;
	
	/* Let's do it */
	this._fnConstruct();
};



Scroller.prototype = {
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Public methods
	 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	
	/**
	 * Calculate the pixel position from the top of the scrolling container for a given row
	 *  @param {int} iRow Row number to calculate the position of
	 *  @returns {int} Pixels
	 *  @example
	 *    $(document).ready(function() {
	 *      $('#example').dataTable( {
	 *        "sScrollY": "200px",
	 *        "sAjaxSource": "media/dataset/large.txt",
	 *        "sDom": "frtiS",
	 *        "bDeferRender": true,
	 *        "fnInitComplete": function (o) {
	 *          // Find where row 25 is
	 *          alert( o.oScroller.fnRowToPixels( 25 ) );
	 *        }
	 *      } );
	 *    } );
	 */
	"fnRowToPixels": function ( iRow )
	{
		return iRow * this.s.rowHeight;
	},


	/**
	 * Calculate the row number that will be found at the given pixel position (y-scroll)
	 *  @param {int} iPixels Offset from top to caluclate the row number of
	 *  @returns {int} Row index
	 *  @example
	 *    $(document).ready(function() {
	 *      $('#example').dataTable( {
	 *        "sScrollY": "200px",
	 *        "sAjaxSource": "media/dataset/large.txt",
	 *        "sDom": "frtiS",
	 *        "bDeferRender": true,
	 *        "fnInitComplete": function (o) {
	 *          // Find what row number is at 500px
	 *          alert( o.oScroller.fnPixelsToRow( 500 ) );
	 *        }
	 *      } );
	 *    } );
	 */
	"fnPixelsToRow": function ( iPixels )
	{
		return parseInt( iPixels / this.s.rowHeight, 10 );
	},


	/**
	 * Calculate the row number that will be found at the given pixel position (y-scroll)
	 *  @param {int} iRow Row index to scroll to
	 *  @param {bool} [bAnimate=true] Animate the transision or not 
	 *  @returns {void}
	 *  @example
	 *    $(document).ready(function() {
	 *      $('#example').dataTable( {
	 *        "sScrollY": "200px",
	 *        "sAjaxSource": "media/dataset/large.txt",
	 *        "sDom": "frtiS",
	 *        "bDeferRender": true,
	 *        "fnInitComplete": function (o) {
	 *          // Immediately scroll to row 1000
	 *          o.oScroller.fnScrollToRow( 1000 );
	 *        }
	 *      } );
	 *      
	 *      // Sometime later on use the following to scroll to row 500...
	 *          var oSettings = $('#example').dataTable().fnSettings();
	 *      oSettings.oScroller.fnScrollToRow( 500 );
	 *    } );
	 */
	"fnScrollToRow": function ( iRow, bAnimate )
	{
		var px = this.fnRowToPixels( iRow );
		if ( typeof bAnimate == 'undefined' || bAnimate )
		{
			$(this.dom.scroller).animate( {
				"scrollTop": px
			} );
		}
		else
		{
			$(this.dom.scroller).scrollTop( px );
		}
	},
	
	
	/**
	 * Calculate and store information about how many rows are to be displayed in the scrolling
	 * viewport, based on current dimensions in the browser's rendering. This can be particularly
	 * useful if the table is initially drawn in a hidden element - for example in a tab.
	 *  @param {bool} [bRedraw=true] Redraw the table automatically after the recalculation, with
	 *    the new dimentions forming the basis for the draw. 
	 *  @returns {void}
	 *  @example
	 *    $(document).ready(function() {
	 *      // Make the example container hidden to throw off the browser's sizing
	 *      document.getElementById('container').style.display = "none";
	 *      var oTable = $('#example').dataTable( {
	 *        "sScrollY": "200px",
	 *        "sAjaxSource": "media/dataset/large.txt",
	 *        "sDom": "frtiS",
	 *        "bDeferRender": true,
	 *        "fnInitComplete": function (o) {
	 *          // Immediately scroll to row 1000
	 *          o.oScroller.fnScrollToRow( 1000 );
	 *        }
	 *      } );
	 *      
	 *      setTimeout( function () {
	 *        // Make the example container visible and recalculate the scroller sizes
	 *        document.getElementById('container').style.display = "block";
	 *        oTable.fnSettings().oScroller.fnMeasure();
	 *      }, 3000 );
	 */
	"fnMeasure": function ( bRedraw )
	{
		if ( this.s.autoHeight )
		{
			this._fnCalcRowHeight();
		}

		this.s.viewportHeight = $(this.dom.scroller).height();
		this.s.viewportRows = parseInt( this.s.viewportHeight/this.s.rowHeight, 10 )+1;
		this.s.dt._iDisplayLength = this.s.viewportRows * this.s.displayBuffer;
		
		if ( this.s.trace )
		{
				console.log(
					'Row height: '+this.s.rowHeight +' '+
					'Viewport height: '+this.s.viewportHeight +' '+
					'Viewport rows: '+ this.s.viewportRows +' '+
					'Display rows: '+ this.s.dt._iDisplayLength
				);
		}
		
		if ( typeof bRedraw == 'undefined' || bRedraw )
		{
			this.s.dt.oInstance.fnDraw();
		}
	},
	
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Private methods (they are of course public in JS, but recommended as private)
	 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	
	/**
	 * Initialisation for Scroller
	 *  @returns {void}
	 *  @private
	 */
	"_fnConstruct": function ()
	{
		var that = this;

		/* Insert a div element that we can use to force the DT scrolling container to
		 * the height that would be required if the whole table was being displayed
		 */
		this.dom.force.style.position = "absolute";
		this.dom.force.style.top = "0px";
		this.dom.force.style.left = "0px";
		this.dom.force.style.width = "1px";

		this.dom.scroller = $('div.'+this.s.dt.oClasses.sScrollBody, this.s.dt.nTableWrapper)[0];
		this.dom.scroller.appendChild( this.dom.force );
		this.dom.scroller.style.position = "relative";

		this.dom.table = $('>table', this.dom.scroller)[0];
		this.dom.table.style.position = "absolute";
		this.dom.table.style.top = "0px";
		this.dom.table.style.left = "0px";

		// Add class to 'announce' that we are a Scroller table
		$(this.s.dt.nTableWrapper).addClass('DTS');

		// Add a 'loading' indicator
		if ( this.s.loadingIndicator )
		{
			$(this.dom.scroller.parentNode)
				.css('position', 'relative')
				.append('<div class="DTS_Loading">'+this.s.dt.oLanguage.sLoadingRecords+'</div>');
		}

		/* Initial size calculations */
		if ( this.s.rowHeight && this.s.rowHeight != 'auto' )
		{
			this.s.autoHeight = false;
		}
		this.fnMeasure( false );

		/* Scrolling callback to see if a page change is needed */
		$(this.dom.scroller).scroll( function () {
			that._fnScroll.call( that );
		} );

		/* In iOS we catch the touchstart event incase the user tries to scroll
		 * while the display is already scrolling
		 */
		$(this.dom.scroller).bind('touchstart', function () {
			that._fnScroll.call( that );
		} );
		
		/* Update the scroller when the DataTable is redrawn */
		this.s.dt.aoDrawCallback.push( {
			"fn": function () {
				if ( that.s.dt.bInitialised ) {
					that._fnDrawCallback.call( that );
				}
			},
			"sName": "Scroller"
		} );
		
		/* Add a state saving parameter to the DT state saving so we can restore the exact
		 * position of the scrolling
		 */
		this.s.dt.oApi._fnCallbackReg( this.s.dt, 'aoStateSaveParams', function (oS, oData) {
			oData.iScroller = that.dom.scroller.scrollTop;
		}, "Scroller_State" );
	},


	/**
	 * Scrolling function - fired whenever the scrolling position is changed. This method needs
	 * to use the stored values to see if the table should be redrawn as we are moving towards
	 * the end of the information that is currently drawn or not. If needed, then it will redraw
	 * the table based on the new position.
	 *  @returns {void}
	 *  @private
	 */
	"_fnScroll": function ()
	{
		var 
			that = this,
			iScrollTop = this.dom.scroller.scrollTop,
			iTopRow;

		/* If the table has been sorted or filtered, then we use the redraw that
		 * DataTables as done, rather than performing our own
		 */
		if ( this.s.dt.bFiltered || this.s.dt.bSorted )
		{
			return;
		}

		if ( this.s.trace )
		{
			console.log(
				'Scroll: '+iScrollTop+'px - boundaries: '+this.s.redrawTop+' / '+this.s.redrawBottom+'. '+
				' Showing rows '+this.fnPixelsToRow(iScrollTop)+
				' to '+this.fnPixelsToRow(iScrollTop+$(this.dom.scroller).height())+
				' in the viewport, with rows '+this.s.dt._iDisplayStart+
				' to '+(this.s.dt._iDisplayEnd)+' rendered by the DataTable'
			);
		}

		/* Update the table's information display for what is now in the viewport */
		this._fnInfo();

		/* We dont' want to state save on every scroll event - that's heavy handed, so
		 * use a timeout to update the state saving only when the scrolling has finished
		 */
		clearTimeout( this.s.stateTO );
		this.s.stateTO = setTimeout( function () {
			that.s.dt.oApi._fnSaveState( that.s.dt );
		}, 250 );

		/* Check if the scroll point is outside the trigger boundary which would required
		 * a DataTables redraw
		 */
		if ( iScrollTop < this.s.redrawTop || iScrollTop > this.s.redrawBottom )
		{
			var preRows = ((this.s.displayBuffer-1)/2) * this.s.viewportRows;
			iTopRow = parseInt( iScrollTop / this.s.rowHeight, 10 ) - preRows;
			if ( iTopRow < 0 )
			{
				/* At the start of the table */
				iTopRow = 0;
			}
			else if ( iTopRow + this.s.dt._iDisplayLength > this.s.dt.fnRecordsDisplay() )
			{
				/* At the end of the table */
				iTopRow = this.s.dt.fnRecordsDisplay() - this.s.dt._iDisplayLength;
				if ( iTopRow < 0 )
				{
					iTopRow = 0;
				}
			}
			else if ( iTopRow % 2 !== 0 )
			{
				/* For the row-striping classes (odd/even) we want only to start on evens
				 * otherwise the stripes will change between draws and look rubbish
				 */
				iTopRow++;
			}

			if ( iTopRow != this.s.dt._iDisplayStart )
			{
				/* Cache the new table position for quick lookups */
				this.s.tableTop = $(this.s.dt.nTable).offset().top;
				this.s.tableBottom = $(this.s.dt.nTable).height() + this.s.tableTop;
				
				/* Do the DataTables redraw based on the calculated start point - note that when
				 * using server-side processing we introduce a small delay to not DoS the server...
				 */
				if ( this.s.dt.oFeatures.bServerSide ) {
					clearTimeout( this.s.drawTO );
					this.s.drawTO = setTimeout( function () {
						that.s.dt._iDisplayStart = iTopRow;
						that.s.dt.oApi._fnCalculateEnd( that.s.dt );
						that.s.dt.oApi._fnDraw( that.s.dt );
					}, this.s.serverWait );
				}
				else
				{
					this.s.dt._iDisplayStart = iTopRow;
					this.s.dt.oApi._fnCalculateEnd( this.s.dt );
					this.s.dt.oApi._fnDraw( this.s.dt );
				}

				if ( this.s.trace )
				{
					console.log( 'Scroll forcing redraw - top DT render row: '+ iTopRow );
				}
			}
		}
	},


	/**
	 * Draw callback function which is fired when the DataTable is redrawn. The main function of
	 * this method is to position the drawn table correctly the scrolling container for the rows
	 * that is displays as a result of the scrolling position.
	 *  @returns {void}
	 *  @private
	 */
	"_fnDrawCallback": function ()
	{
		var
			that = this,
			iScrollTop = this.dom.scroller.scrollTop,
			iScrollBottom = iScrollTop + this.s.viewportHeight;
		
		/* Set the height of the scrolling forcer to be suitable for the number of rows
		 * in this draw
		 */
		this.dom.force.style.height = (this.s.rowHeight * this.s.dt.fnRecordsDisplay())+"px";
		
		/* Calculate the position that the top of the table should be at */
		var iTableTop = (this.s.rowHeight*this.s.dt._iDisplayStart);
		if ( this.s.dt._iDisplayStart === 0 )
		{
			iTableTop = 0;
		}
		else if ( this.s.dt._iDisplayStart === this.s.dt.fnRecordsDisplay() - this.s.dt._iDisplayLength )
		{
			iTableTop = this.s.rowHeight * this.s.dt._iDisplayStart;
		}

		this.dom.table.style.top = iTableTop+"px";

		/* Cache some information for the scroller */
		this.s.tableTop = iTableTop;
		this.s.tableBottom = $(this.s.dt.nTable).height() + this.s.tableTop;

		this.s.redrawTop = iScrollTop - ( (iScrollTop - this.s.tableTop) * this.s.boundaryScale );
		this.s.redrawBottom = iScrollTop + ( (this.s.tableBottom - iScrollBottom) * this.s.boundaryScale );

		if ( this.s.trace )
		{
			console.log(
				"Table redraw. Table top: "+iTableTop+"px "+
				"Table bottom: "+this.s.tableBottom+" "+
				"Scroll boundary top: "+this.s.redrawTop+" "+
				"Scroll boundary bottom: "+this.s.redrawBottom+" "+
				"Rows drawn: "+this.s.dt._iDisplayLength);
		}

		/* Because of the order of the DT callbacks, the info update will
		 * take precidence over the one we want here. So a 'thread' break is
		 * needed
		 */
		setTimeout( function () {
			that._fnInfo.call( that );
		}, 0 );

		/* Restore the scrolling position that was saved by DataTable's state saving
		 * Note that this is done on the second draw when data is Ajax sourced, and the
		 * first draw when DOM soured
		 */
		if ( this.s.dt.oFeatures.bStateSave && this.s.dt.oLoadedState !== null &&
			 typeof this.s.dt.oLoadedState.iScroller != 'undefined' )
		{
			if ( (this.s.dt.sAjaxSource !== null && this.s.dt.iDraw == 2) ||
			     (this.s.dt.sAjaxSource === null && this.s.dt.iDraw == 1) )
			{
				setTimeout( function () {
					$(that.dom.scroller).scrollTop( that.s.dt.oLoadedState.iScroller );
					that.s.redrawTop = that.s.dt.oLoadedState.iScroller - (that.s.viewportHeight/2);
				}, 0 );
			}
		}
	},


	/**
	 * Automatic calculation of table row height. This is just a little tricky here as using
	 * initialisation DataTables has tale the table out of the document, so we need to create
	 * a new table and insert it into the document, calculate the row height and then whip the
	 * table out.
	 *  @returns {void}
	 *  @private
	 */
	"_fnCalcRowHeight": function ()
	{
		var nTable = this.s.dt.nTable.cloneNode( false );
		var nContainer = $(
			'<div class="'+this.s.dt.oClasses.sWrapper+' DTS">'+
				'<div class="'+this.s.dt.oClasses.sScrollWrapper+'">'+
					'<div class="'+this.s.dt.oClasses.sScrollBody+'"></div>'+
				'</div>'+
			'</div>'
		)[0];

		$(nTable).append(
			'<tbody>'+
				'<tr>'+
					'<td>&nbsp;</td>'+
				'</tr>'+
			'</tbody>'
		);

		$('div.'+this.s.dt.oClasses.sScrollBody, nContainer).append( nTable );

		document.body.appendChild( nContainer );
		this.s.rowHeight = $('tbody tr', nTable).outerHeight();
		document.body.removeChild( nContainer );
	},


	/**
	 * Update any information elements that are controlled by the DataTable based on the scrolling
	 * viewport and what rows are visible in it. This function basically acts in the same way as
	 * _fnUpdateInfo in DataTables, and effectively replaces that function.
	 *  @returns {void}
	 *  @private
	 */
	"_fnInfo": function ()
	{
		if ( !this.s.dt.oFeatures.bInfo )
		{
			return;
		}
		
		var
			dt = this.s.dt,
			iScrollTop = this.dom.scroller.scrollTop,
			iStart = this.fnPixelsToRow(iScrollTop)+1, 
			iMax = dt.fnRecordsTotal(),
			iTotal = dt.fnRecordsDisplay(),
			iPossibleEnd = this.fnPixelsToRow(iScrollTop+$(this.dom.scroller).height()),
			iEnd = iTotal < iPossibleEnd ? iTotal : iPossibleEnd,
			sStart = dt.fnFormatNumber( iStart ),
			sEnd = dt.fnFormatNumber( iEnd ),
			sMax = dt.fnFormatNumber( iMax ),
			sTotal = dt.fnFormatNumber( iTotal ),
			sOut;
		
		if ( dt.fnRecordsDisplay() === 0 && 
			   dt.fnRecordsDisplay() == dt.fnRecordsTotal() )
		{
			/* Empty record set */
			sOut = dt.oLanguage.sInfoEmpty+ dt.oLanguage.sInfoPostFix;
		}
		else if ( dt.fnRecordsDisplay() === 0 )
		{
			/* Rmpty record set after filtering */
			sOut = dt.oLanguage.sInfoEmpty +' '+ 
				dt.oLanguage.sInfoFiltered.replace('_MAX_', sMax)+
					dt.oLanguage.sInfoPostFix;
		}
		else if ( dt.fnRecordsDisplay() == dt.fnRecordsTotal() )
		{
			/* Normal record set */
			sOut = dt.oLanguage.sInfo.
					replace('_START_', sStart).
					replace('_END_',   sEnd).
					replace('_TOTAL_', sTotal)+ 
				dt.oLanguage.sInfoPostFix;
		}
		else
		{
			/* Record set after filtering */
			sOut = dt.oLanguage.sInfo.
					replace('_START_', sStart).
					replace('_END_',   sEnd).
					replace('_TOTAL_', sTotal) +' '+ 
				dt.oLanguage.sInfoFiltered.replace('_MAX_', 
					dt.fnFormatNumber(dt.fnRecordsTotal()))+ 
				dt.oLanguage.sInfoPostFix;
		}
		
		var n = dt.aanFeatures.i;
		if ( typeof n != 'undefined' )
		{
			for ( var i=0, iLen=n.length ; i<iLen ; i++ )
			{
				$(n[i]).html( sOut );
			}
		}
	}
};



/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Statics
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */


/**
 * Scroller default settings for initialisation
 *  @namespace
 *  @static
 */
Scroller.oDefaults = {
	/** 
	 * Indicate if Scroller show show trace information on the console or not. This can be 
	 * useful when debugging Scroller or if just curious as to what it is doing, but should
	 * be turned off for production.
	 *  @type     bool
	 *  @default  false
	 *  @static
	 *  @example
	 *    var oTable = $('#example').dataTable( {
	 *        "sScrollY": "200px",
	 *        "sDom": "frtiS",
	 *        "bDeferRender": true,
	 *        "oScroller": {
	 *          "trace": true
	 *        }
	 *    } );
	 */
	"trace": false,

	/** 
	 * Scroller will attempt to automatically calculate the height of rows for it's internal
	 * calculations. However the height that is used can be overridden using this parameter.
	 *  @type     int|string
	 *  @default  auto
	 *  @static
	 *  @example
	 *    var oTable = $('#example').dataTable( {
	 *        "sScrollY": "200px",
	 *        "sDom": "frtiS",
	 *        "bDeferRender": true,
	 *        "oScroller": {
	 *          "rowHeight": 30
	 *        }
	 *    } );
	 */
	"rowHeight": "auto",

	/** 
	 * When using server-side processing, Scroller will wait a small amount of time to allow
	 * the scrolling to finish before requesting more data from the server. This prevents
	 * you from DoSing your own server! The wait time can be configured by this parameter.
	 *  @type     int
	 *  @default  200
	 *  @static
	 *  @example
	 *    var oTable = $('#example').dataTable( {
	 *        "sScrollY": "200px",
	 *        "sDom": "frtiS",
	 *        "bDeferRender": true,
	 *        "oScroller": {
	 *          "serverWait": 100
	 *        }
	 *    } );
	 */
	"serverWait": 200,

	/** 
	 * The display buffer is what Scroller uses to calculate how many rows it should pre-fetch
	 * for scrolling. Scroller automatically adjusts DataTables' display length to pre-fetch
	 * rows that will be shown in "near scrolling" (i.e. just beyond the current display area).
	 * The value is based upon the number of rows that can be displayed in the viewport (i.e. 
	 * a value of 1), and will apply the display range to records before before and after the
	 * current viewport - i.e. a factor of 3 will allow Scroller to pre-fetch 1 viewport's worth
	 * of rows before the current viewport, the current viewport's rows and 1 viewport's worth
	 * of rows after the current viewport. Adjusting this value can be useful for ensuring 
	 * smooth scrolling based on your data set.
	 *  @type     int
	 *  @default  7
	 *  @static
	 *  @example
	 *    var oTable = $('#example').dataTable( {
	 *        "sScrollY": "200px",
	 *        "sDom": "frtiS",
	 *        "bDeferRender": true,
	 *        "oScroller": {
	 *          "displayBuffer": 10
	 *        }
	 *    } );
	 */
	"displayBuffer": 9,

	/** 
	 * Scroller uses the boundary scaling factor to decide when to redraw the table - which it
	 * typically does before you reach the end of the currently loaded data set (in order to
	 * allow the data to look continuous to a user scrolling through the data). If given as 0
	 * then the table will be redrawn whenever the viewport is scrolled, while 1 would not
	 * redraw the table until the currently loaded data has all been shown. You will want 
	 * something in the middle - the default factor of 0.5 is usually suitable.
	 *  @type     float
	 *  @default  0.5
	 *  @static
	 *  @example
	 *    var oTable = $('#example').dataTable( {
	 *        "sScrollY": "200px",
	 *        "sDom": "frtiS",
	 *        "bDeferRender": true,
	 *        "oScroller": {
	 *          "boundaryScale": 0.75
	 *        }
	 *    } );
	 */
	"boundaryScale": 0.5,

	/** 
	 * Show (or not) the loading element in the background of the table. Note that you should
	 * include the dataTables.scroller.css file for this to be displayed correctly.
	 *  @type     boolean
	 *  @default  false
	 *  @static
	 *  @example
	 *    var oTable = $('#example').dataTable( {
	 *        "sScrollY": "200px",
	 *        "sDom": "frtiS",
	 *        "bDeferRender": true,
	 *        "oScroller": {
	 *          "loadingIndicator": true
	 *        }
	 *    } );
	 */
	"loadingIndicator": false
};



/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Constants
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */


/**
 * Name of this class
 *  @type     String
 *  @default  Scroller
 *  @static
 */
Scroller.prototype.CLASS = "Scroller";


/**
 * Scroller version
 *  @type      String
 *  @default   See code
 *  @static
 */
Scroller.VERSION = "1.1.0";
Scroller.prototype.VERSION = Scroller.VERSION;



/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Initialisation
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/*
 * Register a new feature with DataTables
 */
if ( typeof $.fn.dataTable == "function" &&
     typeof $.fn.dataTableExt.fnVersionCheck == "function" &&
     $.fn.dataTableExt.fnVersionCheck('1.9.0') )
{
	$.fn.dataTableExt.aoFeatures.push( {
		"fnInit": function( oDTSettings ) {
			var init = (typeof oDTSettings.oInit.oScroller == 'undefined') ?
				{} : oDTSettings.oInit.oScroller;
			var oScroller = new Scroller( oDTSettings, init );
			return oScroller.dom.wrapper;
		},
		"cFeature": "S",
		"sFeature": "Scroller"
	} );
}
else
{
	alert( "Warning: Scroller requires DataTables 1.9.0 or greater - www.datatables.net/download");
}


// Attach Scroller to DataTables so it can be accessed as an 'extra'
$.fn.dataTable.Scroller = Scroller;

})(jQuery, window, document);
