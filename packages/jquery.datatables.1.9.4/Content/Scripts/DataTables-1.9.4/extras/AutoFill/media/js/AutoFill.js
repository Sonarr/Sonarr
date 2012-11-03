/*
 * File:        AutoFill.js
 * Version:     1.1.2
 * CVS:         $Id$
 * Description: AutoFill for DataTables
 * Author:      Allan Jardine (www.sprymedia.co.uk)
 * Created:     Mon  6 Sep 2010 16:54:41 BST
 * Modified:    $Date$ by $Author$
 * Language:    Javascript
 * License:     GPL v2 or BSD 3 point
 * Project:     DataTables
 * Contact:     www.sprymedia.co.uk/contact
 * 
 * Copyright 2010-2011 Allan Jardine, all rights reserved.
 *
 * This source file is free software, under either the GPL v2 license or a
 * BSD style license, available at:
 *   http://datatables.net/license_gpl2
 *   http://datatables.net/license_bsd
 *
 */

/* Global scope for AutoFill */
var AutoFill;

(function($) {

/** 
 * AutoFill provides Excel like auto fill features for a DataTable
 * @class AutoFill
 * @constructor
 * @param {object} DataTables settings object
 * @param {object} Configuration object for AutoFill
 */
AutoFill = function( oDT, oConfig )
{
	/* Santiy check that we are a new instance */
	if ( !this.CLASS || this.CLASS != "AutoFill" )
	{
		alert( "Warning: AutoFill must be initialised with the keyword 'new'" );
		return;
	}

	if ( !$.fn.dataTableExt.fnVersionCheck('1.7.0') )
	{
		alert( "Warning: AutoFill requires DataTables 1.7 or greater - www.datatables.net/download");
		return;
	}
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Public class variables
	 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	
	/**
	 * @namespace Settings object which contains customisable information for AutoFill instance
	 */
	this.s = {
		/**
		 * @namespace Cached information about the little dragging icon (the filler)
		 */
		"filler": {
			"height": 0,
			"width": 0
		},
		
		/**
		 * @namespace Cached information about the border display
		 */
		"border": {
			"width": 2
		},
		
		/**
		 * @namespace Store for live information for the current drag
		 */
		"drag": {
			"startX": -1,
			"startY": -1,
			"startTd": null,
			"endTd": null,
			"dragging": false
		},
		
		/**
		 * @namespace Data cache for information that we need for scrolling the screen when we near
		 *   the edges
		 */
		"screen": {
			"interval": null,
			"y": 0,
			"height": 0,
			"scrollTop": 0
		},
		
		/**
		 * @namespace Data cache for the position of the DataTables scrolling element (when scrolling
		 *   is enabled)
		 */
		"scroller": {
			"top": 0,
			"bottom": 0
		},
		
		
		/**
		 * @namespace Information stored for each column. An array of objects
		 */
		"columns": []
	};
	
	
	/**
	 * @namespace Common and useful DOM elements for the class instance
	 */
	this.dom = {
		"table": null,
		"filler": null,
		"borderTop": null,
		"borderRight": null,
		"borderBottom": null,
		"borderLeft": null,
		"currentTarget": null
	};
	
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Public class methods
	 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	
	/**
	 * Retreieve the settings object from an instance
	 *  @method fnSettings
	 *  @returns {object} AutoFill settings object
	 */
	this.fnSettings = function () {
		return this.s;
	};
	
	
	/* Constructor logic */
	this._fnInit( oDT, oConfig );
	return this;
};



AutoFill.prototype = {
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Private methods (they are of course public in JS, but recommended as private)
	 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	
	/**
	 * Initialisation
	 *  @method _fnInit
 	 *  @param {object} oDT DataTables settings object
 	 *  @param {object} oConfig Configuration object for AutoFill
	 *  @returns void
	 */
	"_fnInit": function ( oDT, oConfig )
	{
		var
			that = this,
			i, iLen;
		
		/*
		 * Settings
		 */
		this.s.dt = oDT.fnSettings();
		
		this.dom.table = this.s.dt.nTable;
		
		/* Add and configure the columns */
		for ( i=0, iLen=this.s.dt.aoColumns.length ; i<iLen ; i++ )
		{
			this._fnAddColumn( i );
		}
		
		if ( typeof oConfig != 'undefined' && typeof oConfig.aoColumnDefs != 'undefined' )
		{
			this._fnColumnDefs( oConfig.aoColumnDefs );
		}
		
		if ( typeof oConfig != 'undefined' && typeof oConfig.aoColumns != 'undefined' )
		{
			this._fnColumnsAll( oConfig.aoColumns );
		}
		
		
		/*
		 * DOM
		 */
		
		/* Auto Fill click and drag icon */
		var filler = document.createElement('div');
		filler.className = "AutoFill_filler";
		document.body.appendChild( filler );
		this.dom.filler = filler;
		
		filler.style.display = "block";
		this.s.filler.height = $(filler).height();
		this.s.filler.width = $(filler).width();
		filler.style.display = "none";
		
		/* Border display - one div for each side. We can't just use a single one with a border, as
		 * we want the events to effectively pass through the transparent bit of the box
		 */
		var border;
		var appender = document.body;
		if ( that.s.dt.oScroll.sY !== "" )
		{
			that.s.dt.nTable.parentNode.style.position = "relative";
			appender = that.s.dt.nTable.parentNode;
		}
		
		border = document.createElement('div');
		border.className = "AutoFill_border";
		appender.appendChild( border );
		this.dom.borderTop = border;
		
		border = document.createElement('div');
		border.className = "AutoFill_border";
		appender.appendChild( border );
		this.dom.borderRight = border;
		
		border = document.createElement('div');
		border.className = "AutoFill_border";
		appender.appendChild( border );
		this.dom.borderBottom = border;
		
		border = document.createElement('div');
		border.className = "AutoFill_border";
		appender.appendChild( border );
		this.dom.borderLeft = border;
		
		/*
		 * Events
		 */
		
		$(filler).mousedown( function (e) {
			this.onselectstart = function() { return false; };
			that._fnFillerDragStart.call( that, e );
			return false;
		} );
		
		$('tbody>tr>td', this.dom.table).live( 'mouseover mouseout', function (e) {
			that._fnFillerDisplay.call( that, e );
		} );
	},
	
	
	"_fnColumnDefs": function ( aoColumnDefs )
	{
		var
			i, j, k, iLen, jLen, kLen,
			aTargets;
		
		/* Loop over the column defs array - loop in reverse so first instace has priority */
		for ( i=aoColumnDefs.length-1 ; i>=0 ; i-- )
		{
			/* Each column def can target multiple columns, as it is an array */
			aTargets = aoColumnDefs[i].aTargets;
			for ( j=0, jLen=aTargets.length ; j<jLen ; j++ )
			{
				if ( typeof aTargets[j] == 'number' && aTargets[j] >= 0 )
				{
					/* 0+ integer, left to right column counting. */
					this._fnColumnOptions( aTargets[j], aoColumnDefs[i] );
				}
				else if ( typeof aTargets[j] == 'number' && aTargets[j] < 0 )
				{
					/* Negative integer, right to left column counting */
					this._fnColumnOptions( this.s.dt.aoColumns.length+aTargets[j], aoColumnDefs[i] );
				}
				else if ( typeof aTargets[j] == 'string' )
				{
					/* Class name matching on TH element */
					for ( k=0, kLen=this.s.dt.aoColumns.length ; k<kLen ; k++ )
					{
						if ( aTargets[j] == "_all" ||
						     this.s.dt.aoColumns[k].nTh.className.indexOf( aTargets[j] ) != -1 )
						{
							this._fnColumnOptions( k, aoColumnDefs[i] );
						}
					}
				}
			}
		}
	},
		
		
	"_fnColumnsAll": function ( aoColumns )
	{
		for ( var i=0, iLen=this.s.dt.aoColumns.length ; i<iLen ; i++ )
		{
			this._fnColumnOptions( i, aoColumns[i] );
		}
	},
	
	
	"_fnAddColumn": function ( i )
	{
		this.s.columns[i] = {
			"enable": true,
			"read": this._fnReadCell,
			"write": this._fnWriteCell,
			"step": this._fnStep,
			"complete": null
		};
	},
	
	"_fnColumnOptions": function ( i, opts )
	{
		if ( typeof opts.bEnable != 'undefined' )
		{
			this.s.columns[i].enable = opts.bEnable;
		}
		
		if ( typeof opts.fnRead != 'undefined' )
		{
			this.s.columns[i].read = opts.fnRead;
		}
		
		if ( typeof opts.fnWrite != 'undefined' )
		{
			this.s.columns[i].write = opts.fnWrite;
		}
		
		if ( typeof opts.fnStep != 'undefined' )
		{
			this.s.columns[i].step = opts.fnStep;
		}
		
		if ( typeof opts.fnCallback != 'undefined' )
		{
			this.s.columns[i].complete = opts.fnCallback;
		}
	},
	
	
	/**
	 * Find out the coordinates of a given TD cell in a table
	 *  @method  _fnTargetCoords
	 *  @param   {Node} nTd
	 *  @returns {Object} x and y properties, for the position of the cell in the tables DOM
	 */
	"_fnTargetCoords": function ( nTd )
	{
		var nTr = $(nTd).parents('tr')[0];
		
		return {
			"x": $('td', nTr).index(nTd),
			"y": $('tr', nTr.parentNode).index(nTr)
		};
	},
	
	
	/**
	 * Display the border around one or more cells (from start to end)
	 *  @method  _fnUpdateBorder
	 *  @param   {Node} nStart Starting cell
	 *  @param   {Node} nEnd Ending cell
	 *  @returns void
	 */
	"_fnUpdateBorder": function ( nStart, nEnd )
	{
		var
			border = this.s.border.width,
			offsetStart = $(nStart).offset(),
			offsetEnd = $(nEnd).offset(),
			x1 = offsetStart.left - border,
			x2 = offsetEnd.left + $(nEnd).outerWidth(),
			y1 = offsetStart.top - border,
			y2 = offsetEnd.top + $(nEnd).outerHeight(),
			width = offsetEnd.left + $(nEnd).outerWidth() - offsetStart.left + (2*border),
			height = offsetEnd.top + $(nEnd).outerHeight() - offsetStart.top + (2*border),
			oStyle;
		
		if ( this.s.dt.oScroll.sY !== "" )
		{
			/* The border elements are inside the DT scroller - so position relative to that */
			var
				offsetScroll = $(this.s.dt.nTable.parentNode).offset(),
				scrollTop = $(this.s.dt.nTable.parentNode).scrollTop(),
				scrollLeft = $(this.s.dt.nTable.parentNode).scrollLeft();
			
			x1 -= offsetScroll.left - scrollLeft;
			x2 -= offsetScroll.left - scrollLeft;
			y1 -= offsetScroll.top - scrollTop;
			y2 -= offsetScroll.top - scrollTop;
		}
		
		/* Top */
		oStyle = this.dom.borderTop.style;
		oStyle.top = y1+"px";
		oStyle.left = x1+"px";
		oStyle.height = this.s.border.width+"px";
		oStyle.width = width+"px";
		
		/* Bottom */
		oStyle = this.dom.borderBottom.style;
		oStyle.top = y2+"px";
		oStyle.left = x1+"px";
		oStyle.height = this.s.border.width+"px";
		oStyle.width = width+"px";
		
		/* Left */
		oStyle = this.dom.borderLeft.style;
		oStyle.top = y1+"px";
		oStyle.left = x1+"px";
		oStyle.height = height+"px";
		oStyle.width = this.s.border.width+"px";
		
		/* Right */
		oStyle = this.dom.borderRight.style;
		oStyle.top = y1+"px";
		oStyle.left = x2+"px";
		oStyle.height = height+"px";
		oStyle.width = this.s.border.width+"px";
	},
	
	
	/**
	 * Mouse down event handler for starting a drag
	 *  @method  _fnFillerDragStart
	 *  @param   {Object} e Event object
	 *  @returns void
	 */
	"_fnFillerDragStart": function (e)
	{
		var that = this;
		var startingTd = this.dom.currentTarget;
		
		this.s.drag.dragging = true;
		
		that.dom.borderTop.style.display = "block";
		that.dom.borderRight.style.display = "block";
		that.dom.borderBottom.style.display = "block";
		that.dom.borderLeft.style.display = "block";
		
		var coords = this._fnTargetCoords( startingTd );
		this.s.drag.startX = coords.x;
		this.s.drag.startY = coords.y;
		
		this.s.drag.startTd = startingTd;
		this.s.drag.endTd = startingTd;
		
		this._fnUpdateBorder( startingTd, startingTd );
		
		$(document).bind('mousemove.AutoFill', function (e) {
			that._fnFillerDragMove.call( that, e );
		} );
		
		$(document).bind('mouseup.AutoFill', function (e) {
			that._fnFillerFinish.call( that, e );
		} );
		
		/* Scrolling information cache */
		this.s.screen.y = e.pageY;
		this.s.screen.height = $(window).height();
		this.s.screen.scrollTop = $(document).scrollTop();
		
		if ( this.s.dt.oScroll.sY !== "" )
		{
			this.s.scroller.top = $(this.s.dt.nTable.parentNode).offset().top;
			this.s.scroller.bottom = this.s.scroller.top + $(this.s.dt.nTable.parentNode).height();
		}
		
		/* Scrolling handler - we set an interval (which is cancelled on mouse up) which will fire
		 * regularly and see if we need to do any scrolling
		 */
		this.s.screen.interval = setInterval( function () {
			var iScrollTop = $(document).scrollTop();
			var iScrollDelta = iScrollTop - that.s.screen.scrollTop;
			that.s.screen.y += iScrollDelta;
			
			if ( that.s.screen.height - that.s.screen.y + iScrollTop < 50 )
			{
				$('html, body').animate( {
					"scrollTop": iScrollTop + 50
				}, 240, 'linear' );
			}
			else if ( that.s.screen.y - iScrollTop < 50 )
			{
				$('html, body').animate( {
					"scrollTop": iScrollTop - 50
				}, 240, 'linear' );
			}
			
			if ( that.s.dt.oScroll.sY !== "" )
			{
				if ( that.s.screen.y > that.s.scroller.bottom - 50 )
				{
					$(that.s.dt.nTable.parentNode).animate( {
						"scrollTop": $(that.s.dt.nTable.parentNode).scrollTop() + 50
					}, 240, 'linear' );
				}
				else if ( that.s.screen.y < that.s.scroller.top + 50 )
				{
					$(that.s.dt.nTable.parentNode).animate( {
						"scrollTop": $(that.s.dt.nTable.parentNode).scrollTop() - 50
					}, 240, 'linear' );
				}
			}
		}, 250 );
	},
	
	
	/**
	 * Mouse move event handler for during a move. See if we want to update the display based on the
	 * new cursor position
	 *  @method  _fnFillerDragMove
	 *  @param   {Object} e Event object
	 *  @returns void
	 */
	"_fnFillerDragMove": function (e)
	{
		if ( e.target && e.target.nodeName.toUpperCase() == "TD" &&
		 	e.target != this.s.drag.endTd )
		{
			var coords = this._fnTargetCoords( e.target );
			
			if ( coords.x != this.s.drag.startX )
			{
				e.target = $('tbody>tr:eq('+coords.y+')>td:eq('+this.s.drag.startX+')', this.dom.table)[0];
			 	coords = this._fnTargetCoords( e.target );
			}
			
			if ( coords.x == this.s.drag.startX )
			{
				var drag = this.s.drag;
				drag.endTd = e.target;
				
				if ( coords.y >= this.s.drag.startY )
				{
					this._fnUpdateBorder( drag.startTd, drag.endTd );
				}
				else
				{
					this._fnUpdateBorder( drag.endTd, drag.startTd );
				}
				this._fnFillerPosition( e.target );
			}
		}
		
		/* Update the screen information so we can perform scrolling */
		this.s.screen.y = e.pageY;
		this.s.screen.scrollTop = $(document).scrollTop();
		
		if ( this.s.dt.oScroll.sY !== "" )
		{
			this.s.scroller.scrollTop = $(this.s.dt.nTable.parentNode).scrollTop();
			this.s.scroller.top = $(this.s.dt.nTable.parentNode).offset().top;
			this.s.scroller.bottom = this.s.scroller.top + $(this.s.dt.nTable.parentNode).height();
		}
	},
	
	
	/**
	 * Mouse release handler - end the drag and take action to update the cells with the needed values
	 *  @method  _fnFillerFinish
	 *  @param   {Object} e Event object
	 *  @returns void
	 */
	"_fnFillerFinish": function (e)
	{
		var that = this;
		
		$(document).unbind('mousemove.AutoFill');
		$(document).unbind('mouseup.AutoFill');
		
		this.dom.borderTop.style.display = "none";
		this.dom.borderRight.style.display = "none";
		this.dom.borderBottom.style.display = "none";
		this.dom.borderLeft.style.display = "none";
		
		this.s.drag.dragging = false;
		
		clearInterval( this.s.screen.interval );
		
		var coordsStart = this._fnTargetCoords( this.s.drag.startTd );
		var coordsEnd = this._fnTargetCoords( this.s.drag.endTd );
		var aTds = [];
		var bIncrement;
		
		if ( coordsStart.y <= coordsEnd.y )
		{
			bIncrement = true;
			for ( i=coordsStart.y ; i<=coordsEnd.y ; i++ )
			{
				aTds.push( $('tbody>tr:eq('+i+')>td:eq('+coordsStart.x+')', this.dom.table)[0] );
			}
		}
		else
		{
			bIncrement = false;
			for ( i=coordsStart.y ; i>=coordsEnd.y ; i-- )
			{
				aTds.push( $('tbody>tr:eq('+i+')>td:eq('+coordsStart.x+')', this.dom.table)[0] );
			}
		}
		
		
		var iColumn = coordsStart.x;
		var bLast = false;
		var aoEdited = [];
		var sStart = this.s.columns[iColumn].read.call( this, this.s.drag.startTd );
		var oPrepped = this._fnPrep( sStart );
		
		for ( i=0, iLen=aTds.length ; i<iLen ; i++ )
		{
			if ( i==iLen-1 )
			{
				bLast = true;
			}
			
			var original = this.s.columns[iColumn].read.call( this, aTds[i] );
			var step = this.s.columns[iColumn].step.call( this, aTds[i], oPrepped, i, bIncrement, 
				'SPRYMEDIA_AUTOFILL_STEPPER' );
			this.s.columns[iColumn].write.call( this, aTds[i], step, bLast );
			
			aoEdited.push( {
				"td": aTds[i],
				"newValue": step,
				"oldValue": original
			} );
		}
		
		if ( this.s.columns[iColumn].complete !== null )
		{
			this.s.columns[iColumn].complete.call( this, aoEdited );
		}
	},
	
	
	/**
	 * Chunk a string such that it can be filled in by the stepper function
	 *  @method  _fnPrep
	 *  @param   {String} sStr String to prep
	 *  @returns {Object} with parameters, iStart, sStr and sPostFix
	 */
	"_fnPrep": function ( sStr )
	{
		var aMatch = sStr.match(/[\d\.]+/g);
		if ( !aMatch || aMatch.length === 0 )
		{
			return {
				"iStart": 0,
				"sStr": sStr,
				"sPostFix": ""
			};
		}
		
		var sLast = aMatch[ aMatch.length-1 ];
		var num = parseInt(sLast, 10);
		var regex = new RegExp( '^(.*)'+sLast+'(.*?)$' );
		var decimal = sLast.match(/\./) ? "."+sLast.split('.')[1] : "";
		
		return {
			"iStart": num,
			"sStr": sStr.replace(regex, "$1SPRYMEDIA_AUTOFILL_STEPPER$2"),
			"sPostFix": decimal
		};
	},
	
	
	/**
	 * Render a string for it's position in the table after the drag (incrememt numbers)
	 *  @method  _fnStep
	 *  @param   {Node} nTd Cell being written to
	 *  @param   {Object} oPrepped Prepared object for the stepper (from _fnPrep)
	 *  @param   {Int} iDiff Step difference
	 *  @param   {Boolean} bIncrement Increment (true) or decriment (false)
	 *  @param   {String} sToken Token to replace
	 *  @returns {String} Rendered information
	 */
	"_fnStep": function ( nTd, oPrepped, iDiff, bIncrement, sToken )
	{
		var iReplace = bIncrement ? (oPrepped.iStart+iDiff) : (oPrepped.iStart-iDiff);
		if ( isNaN(iReplace) )
		{
			iReplace = "";
		}
		return oPrepped.sStr.replace( sToken, iReplace+oPrepped.sPostFix );
	},
	
	
	/**
	 * Read informaiton from a cell, possibly using live DOM elements if suitable
	 *  @method  _fnReadCell
	 *  @param   {Node} nTd Cell to read
	 *  @returns {String} Read value
	 */
	"_fnReadCell": function ( nTd )
	{
		var jq = $('input', nTd);
		if ( jq.length > 0 )
		{
			return $(jq).val();
		}
		
		jq = $('select', nTd);
		if ( jq.length > 0 )
		{
			return $(jq).val();
		}
		
		return nTd.innerHTML;
	},
	
	
	/**
	 * Write informaiton to a cell, possibly using live DOM elements if suitable
	 *  @method  _fnWriteCell
	 *  @param   {Node} nTd Cell to write
	 *  @param   {String} sVal Value to write
	 *  @param   {Boolean} bLast Flag to show if this is that last update
	 *  @returns void
	 */
	"_fnWriteCell": function ( nTd, sVal, bLast )
	{
		var jq = $('input', nTd);
		if ( jq.length > 0 )
		{
			$(jq).val( sVal );
			return;
		}
		
		jq = $('select', nTd);
		if ( jq.length > 0 )
		{
			$(jq).val( sVal );
			return;
		}
		
		var pos = this.s.dt.oInstance.fnGetPosition( nTd );
		this.s.dt.oInstance.fnUpdate( sVal, pos[0], pos[2], bLast );
	},
	
	
	/**
	 * Display the drag handle on mouse over cell
	 *  @method  _fnFillerDisplay
	 *  @param   {Object} e Event object
	 *  @returns void
	 */
	"_fnFillerDisplay": function (e)
	{
		/* Don't display automatically when dragging */
		if ( this.s.drag.dragging)
		{
			return;
		}
		
		/* Check that we are allowed to AutoFill this column or not */
		var nTd = (e.target.nodeName.toLowerCase() == 'td') ? e.target : $(e.target).parents('td')[0];
		var iX = this._fnTargetCoords(nTd).x;
		if ( !this.s.columns[iX].enable )
		{
			return;
		}
		
		var filler = this.dom.filler;
		if (e.type == 'mouseover')
		{
			this.dom.currentTarget = nTd;
			this._fnFillerPosition( nTd );
			
			filler.style.display = "block";
		}
		else if ( !e.relatedTarget || !e.relatedTarget.className.match(/AutoFill/) )
		{
			filler.style.display = "none";
		}
	},
	
	
	/**
	 * Position the filler icon over a cell
	 *  @method  _fnFillerPosition
	 *  @param   {Node} nTd Cell to position filler icon over
	 *  @returns void
	 */
	"_fnFillerPosition": function ( nTd )
	{
		var offset = $(nTd).offset();
		var filler = this.dom.filler;
		filler.style.top = (offset.top - (this.s.filler.height / 2)-1 + $(nTd).outerHeight())+"px";
		filler.style.left = (offset.left - (this.s.filler.width / 2)-1 + $(nTd).outerWidth())+"px";
	}
};




/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Constants
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/**
 * Name of this class
 *  @constant CLASS
 *  @type     String
 *  @default  AutoFill
 */
AutoFill.prototype.CLASS = "AutoFill";


/**
 * AutoFill version
 *  @constant  VERSION
 *  @type      String
 *  @default   1.1.2
 */
AutoFill.VERSION = "1.1.2";
AutoFill.prototype.VERSION = AutoFill.VERSION;


})(jQuery);