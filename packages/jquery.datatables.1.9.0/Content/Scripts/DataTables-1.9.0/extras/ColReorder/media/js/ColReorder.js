/*
 * File:        ColReorder.js
 * Version:     1.0.5
 * CVS:         $Id$
 * Description: Controls for column visiblity in DataTables
 * Author:      Allan Jardine (www.sprymedia.co.uk)
 * Created:     Wed Sep 15 18:23:29 BST 2010
 * Modified:    $Date$ by $Author$
 * Language:    Javascript
 * License:     GPL v2 or BSD 3 point style
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


(function($, window, document) {


/**
 * Switch the key value pairing of an index array to be value key (i.e. the old value is now the
 * key). For example consider [ 2, 0, 1 ] this would be returned as [ 1, 2, 0 ].
 *  @method  fnInvertKeyValues
 *  @param   array aIn Array to switch around
 *  @returns array
 */
function fnInvertKeyValues( aIn )
{
	var aRet=[];
	for ( var i=0, iLen=aIn.length ; i<iLen ; i++ )
	{
		aRet[ aIn[i] ] = i;
	}
	return aRet;
}


/**
 * Modify an array by switching the position of two elements
 *  @method  fnArraySwitch
 *  @param   array aArray Array to consider, will be modified by reference (i.e. no return)
 *  @param   int iFrom From point
 *  @param   int iTo Insert point
 *  @returns void
 */
function fnArraySwitch( aArray, iFrom, iTo )
{
	var mStore = aArray.splice( iFrom, 1 )[0];
	aArray.splice( iTo, 0, mStore );
}


/**
 * Switch the positions of nodes in a parent node (note this is specifically designed for 
 * table rows). Note this function considers all element nodes under the parent!
 *  @method  fnDomSwitch
 *  @param   string sTag Tag to consider
 *  @param   int iFrom Element to move
 *  @param   int Point to element the element to (before this point), can be null for append
 *  @returns void
 */
function fnDomSwitch( nParent, iFrom, iTo )
{
	var anTags = [];
	for ( var i=0, iLen=nParent.childNodes.length ; i<iLen ; i++ )
	{
		if ( nParent.childNodes[i].nodeType == 1 )
		{
			anTags.push( nParent.childNodes[i] );
		}
	}
	var nStore = anTags[ iFrom ];
	
	if ( iTo !== null )
	{
		nParent.insertBefore( nStore, anTags[iTo] );
	}
	else
	{
		nParent.appendChild( nStore );
	}
}



/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * DataTables plug-in API functions
 *
 * This are required by ColReorder in order to perform the tasks required, and also keep this
 * code portable, to be used for other column reordering projects with DataTables, if needed.
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */


/**
 * Plug-in for DataTables which will reorder the internal column structure by taking the column
 * from one position (iFrom) and insert it into a given point (iTo).
 *  @method  $.fn.dataTableExt.oApi.fnColReorder
 *  @param   object oSettings DataTables settings object - automatically added by DataTables! 
 *  @param   int iFrom Take the column to be repositioned from this point
 *  @param   int iTo and insert it into this point
 *  @returns void
 */
$.fn.dataTableExt.oApi.fnColReorder = function ( oSettings, iFrom, iTo )
{
	var i, iLen, j, jLen, iCols=oSettings.aoColumns.length, nTrs, oCol;
	
	/* Sanity check in the input */
	if ( iFrom == iTo )
	{
		/* Pointless reorder */
		return;
	}
	
	if ( iFrom < 0 || iFrom >= iCols )
	{
		this.oApi._fnLog( oSettings, 1, "ColReorder 'from' index is out of bounds: "+iFrom );
		return;
	}
	
	if ( iTo < 0 || iTo >= iCols )
	{
		this.oApi._fnLog( oSettings, 1, "ColReorder 'to' index is out of bounds: "+iTo );
		return;
	}
	
	/*
	 * Calculate the new column array index, so we have a mapping between the old and new
	 */
	var aiMapping = [];
	for ( i=0, iLen=iCols ; i<iLen ; i++ )
	{
		aiMapping[i] = i;
	}
	fnArraySwitch( aiMapping, iFrom, iTo );
	var aiInvertMapping = fnInvertKeyValues( aiMapping );
	
	
	/*
	 * Convert all internal indexing to the new column order indexes
	 */
	/* Sorting */
	for ( i=0, iLen=oSettings.aaSorting.length ; i<iLen ; i++ )
	{
		oSettings.aaSorting[i][0] = aiInvertMapping[ oSettings.aaSorting[i][0] ];
	}
	
	/* Fixed sorting */
	if ( oSettings.aaSortingFixed !== null )
	{
		for ( i=0, iLen=oSettings.aaSortingFixed.length ; i<iLen ; i++ )
		{
			oSettings.aaSortingFixed[i][0] = aiInvertMapping[ oSettings.aaSortingFixed[i][0] ];
		}
	}
	
	/* Data column sorting (the column which the sort for a given column should take place on) */
	for ( i=0, iLen=iCols ; i<iLen ; i++ )
	{
		oCol = oSettings.aoColumns[i];
		for ( j=0, jLen=oCol.aDataSort.length ; j<jLen ; j++ )
		{
			oCol.aDataSort[j] = aiInvertMapping[ oCol.aDataSort[j] ];
		}
	}
	
	/* Update the Get and Set functions for each column */
	for ( i=0, iLen=iCols ; i<iLen ; i++ )
	{
		oCol = oSettings.aoColumns[i];
		if ( typeof oCol.mDataProp == 'number' ) {
			oCol.mDataProp = aiInvertMapping[ oCol.mDataProp ];
			oCol.fnGetData = oSettings.oApi._fnGetObjectDataFn( oCol.mDataProp );
			oCol.fnSetData = oSettings.oApi._fnSetObjectDataFn( oCol.mDataProp );
		}
	}
	
	
	/*
	 * Move the DOM elements
	 */
	if ( oSettings.aoColumns[iFrom].bVisible )
	{
		/* Calculate the current visible index and the point to insert the node before. The insert
		 * before needs to take into account that there might not be an element to insert before,
		 * in which case it will be null, and an appendChild should be used
		 */
		var iVisibleIndex = this.oApi._fnColumnIndexToVisible( oSettings, iFrom );
		var iInsertBeforeIndex = null;
		
		i = iTo < iFrom ? iTo : iTo + 1;
		while ( iInsertBeforeIndex === null && i < iCols )
		{
			iInsertBeforeIndex = this.oApi._fnColumnIndexToVisible( oSettings, i );
			i++;
		}
		
		/* Header */
		nTrs = oSettings.nTHead.getElementsByTagName('tr');
		for ( i=0, iLen=nTrs.length ; i<iLen ; i++ )
		{
			fnDomSwitch( nTrs[i], iVisibleIndex, iInsertBeforeIndex );
		}
		
		/* Footer */
		if ( oSettings.nTFoot !== null )
		{
			nTrs = oSettings.nTFoot.getElementsByTagName('tr');
			for ( i=0, iLen=nTrs.length ; i<iLen ; i++ )
			{
				fnDomSwitch( nTrs[i], iVisibleIndex, iInsertBeforeIndex );
			}
		}
		
		/* Body */
		for ( i=0, iLen=oSettings.aoData.length ; i<iLen ; i++ )
		{
			if ( oSettings.aoData[i].nTr !== null )
			{
				fnDomSwitch( oSettings.aoData[i].nTr, iVisibleIndex, iInsertBeforeIndex );
			}
		}
	}
	
	
	/* 
	 * Move the internal array elements
	 */
	/* Columns */
	fnArraySwitch( oSettings.aoColumns, iFrom, iTo );
	
	/* Search columns */
	fnArraySwitch( oSettings.aoPreSearchCols, iFrom, iTo );
	
	/* Array array - internal data anodes cache */
	for ( i=0, iLen=oSettings.aoData.length ; i<iLen ; i++ )
	{
		if ( $.isArray( oSettings.aoData[i]._aData ) ) {
		  fnArraySwitch( oSettings.aoData[i]._aData, iFrom, iTo );
		}
		fnArraySwitch( oSettings.aoData[i]._anHidden, iFrom, iTo );
	}
	
	/* Reposition the header elements in the header layout array */
	for ( i=0, iLen=oSettings.aoHeader.length ; i<iLen ; i++ )
	{
		fnArraySwitch( oSettings.aoHeader[i], iFrom, iTo );
	}
	
	if ( oSettings.aoFooter !== null )
	{
		for ( i=0, iLen=oSettings.aoFooter.length ; i<iLen ; i++ )
		{
			fnArraySwitch( oSettings.aoFooter[i], iFrom, iTo );
		}
	}
	
	
	/*
	 * Update DataTables' event handlers
	 */
	
	/* Sort listener */
	for ( i=0, iLen=iCols ; i<iLen ; i++ )
	{
		$(oSettings.aoColumns[i].nTh).unbind('click');
		this.oApi._fnSortAttachListener( oSettings, oSettings.aoColumns[i].nTh, i );
	}
	
	
	/*
	 * Any extra operations for the other plug-ins
	 */
	if ( typeof ColVis != 'undefined' )
	{
		ColVis.fnRebuild( oSettings.oInstance );
	}
	
	if ( typeof oSettings.oInstance._oPluginFixedHeader != 'undefined' )
	{
		oSettings.oInstance._oPluginFixedHeader.fnUpdate();
	}
};




/** 
 * ColReorder provides column visiblity control for DataTables
 * @class ColReorder
 * @constructor
 * @param {object} DataTables object
 * @param {object} ColReorder options
 */
ColReorder = function( oTable, oOpts )
{
	/* Santiy check that we are a new instance */
	if ( !this.CLASS || this.CLASS != "ColReorder" )
	{
		alert( "Warning: ColReorder must be initialised with the keyword 'new'" );
	}
	
	if ( typeof oOpts == 'undefined' )
	{
		oOpts = {};
	}
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Public class variables
	 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	
	/**
	 * @namespace Settings object which contains customisable information for ColReorder instance
	 */
	this.s = {
		/**
		 * DataTables settings object
		 *  @property dt
		 *  @type     Object
		 *  @default  null
		 */
		"dt": null,
		
		/**
		 * Initialisation object used for this instance
		 *  @property init
		 *  @type     object
		 *  @default  {}
		 */
		"init": oOpts,
		
		/**
		 * Number of columns to fix (not allow to be reordered)
		 *  @property fixed
		 *  @type     int
		 *  @default  0
		 */
		"fixed": 0,
		
		/**
		 * Callback function for once the reorder has been done
		 *  @property dropcallback
		 *  @type     function
		 *  @default  null
		 */
		"dropCallback": null,
		
		/**
		 * @namespace Information used for the mouse drag
		 */
		"mouse": {
			"startX": -1,
			"startY": -1,
			"offsetX": -1,
			"offsetY": -1,
			"target": -1,
			"targetIndex": -1,
			"fromIndex": -1
		},
		
		/**
		 * Information which is used for positioning the insert cusor and knowing where to do the
		 * insert. Array of objects with the properties:
		 *   x: x-axis position
		 *   to: insert point
		 *  @property aoTargets
		 *  @type     array
		 *  @default  []
		 */
		"aoTargets": []
	};
	
	
	/**
	 * @namespace Common and useful DOM elements for the class instance
	 */
	this.dom = {
		/**
		 * Dragging element (the one the mouse is moving)
		 *  @property drag
		 *  @type     element
		 *  @default  null
		 */
		"drag": null,
		
		/**
		 * The insert cursor
		 *  @property pointer
		 *  @type     element
		 *  @default  null
		 */
		"pointer": null
	};
	
	
	/* Constructor logic */
	this.s.dt = oTable.fnSettings();
	this._fnConstruct();
	
	/* Store the instance for later use */
	ColReorder.aoInstances.push( this );
	return this;
};



ColReorder.prototype = {
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Public methods
	 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	
	"fnReset": function ()
	{
		var a = [];
		for ( var i=0, iLen=this.s.dt.aoColumns.length ; i<iLen ; i++ )
		{
			a.push( this.s.dt.aoColumns[i]._ColReorder_iOrigCol );
		}
		
		this._fnOrderColumns( a );
	},
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Private methods (they are of course public in JS, but recommended as private)
	 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
	
	/**
	 * Constructor logic
	 *  @method  _fnConstruct
	 *  @returns void
	 *  @private 
	 */
	"_fnConstruct": function ()
	{
		var that = this;
		var i, iLen;
		
		/* Columns discounted from reordering - counting left to right */
		if ( typeof this.s.init.iFixedColumns != 'undefined' )
		{
			this.s.fixed = this.s.init.iFixedColumns;
		}
		
		/* Drop callback initialisation option */
		if ( typeof this.s.init.fnReorderCallback != 'undefined' )
		{
			this.s.dropCallback = this.s.init.fnReorderCallback;
		}
		
		/* Add event handlers for the drag and drop, and also mark the original column order */
		for ( i=0, iLen=this.s.dt.aoColumns.length ; i<iLen ; i++ )
		{
			if ( i > this.s.fixed-1 )
			{
				this._fnMouseListener( i, this.s.dt.aoColumns[i].nTh );
			}
			
			/* Mark the original column order for later reference */
			this.s.dt.aoColumns[i]._ColReorder_iOrigCol = i;
		}
		
		/* State saving */
		this.s.dt.oApi._fnCallbackReg( this.s.dt, 'aoStateSaveParams', function (oS, oData) {
			that._fnStateSave.call( that, oData );
		}, "ColReorder_State" );
		
		/* An initial column order has been specified */
		var aiOrder = null;
		if ( typeof this.s.init.aiOrder != 'undefined' )
		{
			aiOrder = this.s.init.aiOrder.slice();
		}
		
		/* State loading, overrides the column order given */
		if ( this.s.dt.oLoadedState && typeof this.s.dt.oLoadedState.ColReorder != 'undefined' &&
		  this.s.dt.oLoadedState.ColReorder.length == this.s.dt.aoColumns.length )
		{
			aiOrder = this.s.dt.oLoadedState.ColReorder;
		}
		
		/* If we have an order to apply - do so */
		if ( aiOrder )
		{
			/* We might be called during or after the DataTables initialisation. If before, then we need
			 * to wait until the draw is done, if after, then do what we need to do right away
			 */
			if ( !that.s.dt._bInitComplete )
			{
				var bDone = false;
				this.s.dt.aoDrawCallback.push( {
					"fn": function () {
						if ( !that.s.dt._bInitComplete && !bDone )
						{
							bDone = true;
							var resort = fnInvertKeyValues( aiOrder );
							that._fnOrderColumns.call( that, resort );
						}
					},
					"sName": "ColReorder_Pre"
				} );
			}
			else
			{
				var resort = fnInvertKeyValues( aiOrder );
				that._fnOrderColumns.call( that, resort );
			}
		}
	},
	
	
	/**
	 * Set the column order from an array
	 *  @method  _fnOrderColumns
	 *  @param   array a An array of integers which dictate the column order that should be applied
	 *  @returns void
	 *  @private 
	 */
	"_fnOrderColumns": function ( a )
	{
		if ( a.length != this.s.dt.aoColumns.length )
		{
			this.s.dt.oInstance.oApi._fnLog( oDTSettings, 1, "ColReorder - array reorder does not "+
			 	"match known number of columns. Skipping." );
			return;
		}
		
		for ( var i=0, iLen=a.length ; i<iLen ; i++ )
		{
			var currIndex = $.inArray( i, a );
			if ( i != currIndex )
			{
				/* Reorder our switching array */
				fnArraySwitch( a, currIndex, i );
				
				/* Do the column reorder in the table */
				this.s.dt.oInstance.fnColReorder( currIndex, i );
			}
		}
		
		/* When scrolling we need to recalculate the column sizes to allow for the shift */
		if ( this.s.dt.oScroll.sX !== "" || this.s.dt.oScroll.sY !== "" )
		{
			this.s.dt.oInstance.fnAdjustColumnSizing();
		}
			
		/* Save the state */
		this.s.dt.oInstance.oApi._fnSaveState( this.s.dt );
	},
	
	
	/**
	 * Because we change the indexes of columns in the table, relative to their starting point
	 * we need to reorder the state columns to what they are at the starting point so we can
	 * then rearrange them again on state load!
	 *  @method  _fnStateSave
	 *  @param   object oState DataTables state 
	 *  @returns string JSON encoded cookie string for DataTables
	 *  @private 
	 */
	"_fnStateSave": function ( oState )
	{
		var i, iLen, aCopy, iOrigColumn;
		var oSettings = this.s.dt;

		/* Sorting */
		for ( i=0 ; i<oState.aaSorting.length ; i++ )
		{
			oState.aaSorting[i][0] = oSettings.aoColumns[ oState.aaSorting[i][0] ]._ColReorder_iOrigCol;
		}

		aSearchCopy = $.extend( true, [], oState.aoSearchCols );
		oState.ColReorder = [];

		for ( i=0, iLen=oSettings.aoColumns.length ; i<iLen ; i++ )
		{
			iOrigColumn = oSettings.aoColumns[i]._ColReorder_iOrigCol;

			/* Column filter */
			oState.aoSearchCols[ iOrigColumn ] = aSearchCopy[i];

			/* Visibility */
			oState.abVisCols[ iOrigColumn ] = oSettings.aoColumns[i].bVisible;
		
			/* Column reordering */
			oState.ColReorder.push( iOrigColumn );
		}
	},
	
	
	/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
	 * Mouse drop and drag
	 */
	
	/**
	 * Add a mouse down listener to a particluar TH element
	 *  @method  _fnMouseListener
	 *  @param   int i Column index
	 *  @param   element nTh TH element clicked on
	 *  @returns void
	 *  @private 
	 */
	"_fnMouseListener": function ( i, nTh )
	{
		var that = this;
		$(nTh).bind( 'mousedown.ColReorder', function (e) {
			that._fnMouseDown.call( that, e, nTh );
			return false;
		} );
	},
	
	
	/**
	 * Mouse down on a TH element in the table header
	 *  @method  _fnMouseDown
	 *  @param   event e Mouse event
	 *  @param   element nTh TH element to be dragged
	 *  @returns void
	 *  @private 
	 */
	"_fnMouseDown": function ( e, nTh )
	{
		var
			that = this,
			aoColumns = this.s.dt.aoColumns;
		
		/* Store information about the mouse position */
		var nThTarget = e.target.nodeName == "TH" ? e.target : $(e.target).parents('TH')[0];
		var offset = $(nThTarget).offset();
		this.s.mouse.startX = e.pageX;
		this.s.mouse.startY = e.pageY;
		this.s.mouse.offsetX = e.pageX - offset.left;
		this.s.mouse.offsetY = e.pageY - offset.top;
		this.s.mouse.target = nTh;
		this.s.mouse.targetIndex = $('th', nTh.parentNode).index( nTh );
		this.s.mouse.fromIndex = this.s.dt.oInstance.oApi._fnVisibleToColumnIndex( this.s.dt, 
			this.s.mouse.targetIndex );
		
		/* Calculate a cached array with the points of the column inserts, and the 'to' points */
		this.s.aoTargets.splice( 0, this.s.aoTargets.length );
		
		this.s.aoTargets.push( {
			"x":  $(this.s.dt.nTable).offset().left,
			"to": 0
		} );
		
		var iToPoint = 0;
		for ( var i=0, iLen=aoColumns.length ; i<iLen ; i++ )
		{
			/* For the column / header in question, we want it's position to remain the same if the 
			 * position is just to it's immediate left or right, so we only incremement the counter for
			 * other columns
			 */
			if ( i != this.s.mouse.fromIndex )
			{
				iToPoint++;
			}
			
			if ( aoColumns[i].bVisible )
			{
				this.s.aoTargets.push( {
					"x":  $(aoColumns[i].nTh).offset().left + $(aoColumns[i].nTh).outerWidth(),
					"to": iToPoint
				} );
			}
		}
		
		/* Disallow columns for being reordered by drag and drop, counting left to right */
		if ( this.s.fixed !== 0 )
		{
			this.s.aoTargets.splice( 0, this.s.fixed );
		}
		
		/* Add event handlers to the document */
		$(document).bind( 'mousemove.ColReorder', function (e) {
			that._fnMouseMove.call( that, e );
		} );
		
		$(document).bind( 'mouseup.ColReorder', function (e) {
			that._fnMouseUp.call( that, e );
		} );
	},
	
	
	/**
	 * Deal with a mouse move event while dragging a node
	 *  @method  _fnMouseMove
	 *  @param   event e Mouse event
	 *  @returns void
	 *  @private 
	 */
	"_fnMouseMove": function ( e )
	{
		var that = this;
		
		if ( this.dom.drag === null )
		{
			/* Only create the drag element if the mouse has moved a specific distance from the start
			 * point - this allows the user to make small mouse movements when sorting and not have a
			 * possibly confusing drag element showing up
			 */
			if ( Math.pow(
				Math.pow(e.pageX - this.s.mouse.startX, 2) + 
				Math.pow(e.pageY - this.s.mouse.startY, 2), 0.5 ) < 5 )
			{
				return;
			}
			this._fnCreateDragNode();
		}
		
		/* Position the element - we respect where in the element the click occured */
		this.dom.drag.style.left = (e.pageX - this.s.mouse.offsetX) + "px";
		this.dom.drag.style.top = (e.pageY - this.s.mouse.offsetY) + "px";
		
		/* Based on the current mouse position, calculate where the insert should go */
		var bSet = false;
		for ( var i=1, iLen=this.s.aoTargets.length ; i<iLen ; i++ )
		{
			if ( e.pageX < this.s.aoTargets[i-1].x + ((this.s.aoTargets[i].x-this.s.aoTargets[i-1].x)/2) )
			{
				this.dom.pointer.style.left = this.s.aoTargets[i-1].x +"px";
				this.s.mouse.toIndex = this.s.aoTargets[i-1].to;
				bSet = true;
				break;
			}
		}
		
		/* The insert element wasn't positioned in the array (less than operator), so we put it at 
		 * the end
		 */
		if ( !bSet )
		{
			this.dom.pointer.style.left = this.s.aoTargets[this.s.aoTargets.length-1].x +"px";
			this.s.mouse.toIndex = this.s.aoTargets[this.s.aoTargets.length-1].to;
		}
	},
	
	
	/**
	 * Finish off the mouse drag and insert the column where needed
	 *  @method  _fnMouseUp
	 *  @param   event e Mouse event
	 *  @returns void
	 *  @private 
	 */
	"_fnMouseUp": function ( e )
	{
		var that = this;
		
		$(document).unbind( 'mousemove.ColReorder' );
		$(document).unbind( 'mouseup.ColReorder' );
		
		if ( this.dom.drag !== null )
		{
			/* Remove the guide elements */
			document.body.removeChild( this.dom.drag );
			document.body.removeChild( this.dom.pointer );
			this.dom.drag = null;
			this.dom.pointer = null;
			
			/* Actually do the reorder */
			this.s.dt.oInstance.fnColReorder( this.s.mouse.fromIndex, this.s.mouse.toIndex );
			
			/* When scrolling we need to recalculate the column sizes to allow for the shift */
			if ( this.s.dt.oScroll.sX !== "" || this.s.dt.oScroll.sY !== "" )
			{
				this.s.dt.oInstance.fnAdjustColumnSizing();
			}
			
			if ( this.s.dropCallback !== null )
			{
				this.s.dropCallback.call( this );
			}
			
			/* Save the state */
			this.s.dt.oInstance.oApi._fnSaveState( this.s.dt );
		}
	},
	
	
	/**
	 * Copy the TH element that is being drags so the user has the idea that they are actually 
	 * moving it around the page.
	 *  @method  _fnCreateDragNode
	 *  @returns void
	 *  @private 
	 */
	"_fnCreateDragNode": function ()
	{
		var that = this;
		
		this.dom.drag = $(this.s.dt.nTHead.parentNode).clone(true)[0];
		this.dom.drag.className += " DTCR_clonedTable";
		while ( this.dom.drag.getElementsByTagName('caption').length > 0 )
		{
			this.dom.drag.removeChild( this.dom.drag.getElementsByTagName('caption')[0] );
		}
		while ( this.dom.drag.getElementsByTagName('tbody').length > 0 )
		{
			this.dom.drag.removeChild( this.dom.drag.getElementsByTagName('tbody')[0] );
		}
		while ( this.dom.drag.getElementsByTagName('tfoot').length > 0 )
		{
			this.dom.drag.removeChild( this.dom.drag.getElementsByTagName('tfoot')[0] );
		}
		
		$('thead tr:eq(0)', this.dom.drag).each( function () {
			$('th:not(:eq('+that.s.mouse.targetIndex+'))', this).remove();
		} );
		$('tr', this.dom.drag).height( $('tr:eq(0)', that.s.dt.nTHead).height() );
		
		$('thead tr:gt(0)', this.dom.drag).remove();
		
		$('thead th:eq(0)', this.dom.drag).each( function (i) {
			this.style.width = $('th:eq('+that.s.mouse.targetIndex+')', that.s.dt.nTHead).width()+"px";
		} );
		
		this.dom.drag.style.position = "absolute";
		this.dom.drag.style.top = "0px";
		this.dom.drag.style.left = "0px";
		this.dom.drag.style.width = $('th:eq('+that.s.mouse.targetIndex+')', that.s.dt.nTHead).outerWidth()+"px";
		
		
		this.dom.pointer = document.createElement( 'div' );
		this.dom.pointer.className = "DTCR_pointer";
		this.dom.pointer.style.position = "absolute";
		
		if ( this.s.dt.oScroll.sX === "" && this.s.dt.oScroll.sY === "" )
		{
			this.dom.pointer.style.top = $(this.s.dt.nTable).offset().top+"px";
			this.dom.pointer.style.height = $(this.s.dt.nTable).height()+"px";
		}
		else
		{
			this.dom.pointer.style.top = $('div.dataTables_scroll', this.s.dt.nTableWrapper).offset().top+"px";
			this.dom.pointer.style.height = $('div.dataTables_scroll', this.s.dt.nTableWrapper).height()+"px";
		}
	
		document.body.appendChild( this.dom.pointer );
		document.body.appendChild( this.dom.drag );
	}
};





/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Static parameters
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/**
 * Array of all ColReorder instances for later reference
 *  @property ColReorder.aoInstances
 *  @type     array
 *  @default  []
 *  @static
 */
ColReorder.aoInstances = [];





/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Static functions
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/**
 * Reset the column ordering for a DataTables instance
 *  @method  ColReorder.fnReset
 *  @param   object oTable DataTables instance to consider
 *  @returns void
 *  @static
 */
ColReorder.fnReset = function ( oTable )
{
	for ( var i=0, iLen=ColReorder.aoInstances.length ; i<iLen ; i++ )
	{
		if ( ColReorder.aoInstances[i].s.dt.oInstance == oTable )
		{
			ColReorder.aoInstances[i].fnReset();
		}
	}
};





/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Constants
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/**
 * Name of this class
 *  @constant CLASS
 *  @type     String
 *  @default  ColReorder
 */
ColReorder.prototype.CLASS = "ColReorder";


/**
 * ColReorder version
 *  @constant  VERSION
 *  @type      String
 *  @default   As code
 */
ColReorder.VERSION = "1.0.5";
ColReorder.prototype.VERSION = ColReorder.VERSION;





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
			var oTable = oDTSettings.oInstance;
			if ( typeof oTable._oPluginColReorder == 'undefined' ) {
				var opts = typeof oDTSettings.oInit.oColReorder != 'undefined' ? 
					oDTSettings.oInit.oColReorder : {};
				oTable._oPluginColReorder = new ColReorder( oDTSettings.oInstance, opts );
			} else {
				oTable.oApi._fnLog( oDTSettings, 1, "ColReorder attempted to initialise twice. Ignoring second" );
			}
			
			return null; /* No node to insert */
		},
		"cFeature": "R",
		"sFeature": "ColReorder"
	} );
}
else
{
	alert( "Warning: ColReorder requires DataTables 1.9.0 or greater - www.datatables.net/download");
}

})(jQuery, window, document);
