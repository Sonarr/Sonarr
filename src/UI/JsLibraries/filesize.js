/**
 * filesize
 *
 * @author Jason Mulligan <jason.mulligan@avoidwork.com>
 * @copyright 2013 Jason Mulligan
 * @license BSD-3 <https://raw.github.com/avoidwork/filesize.js/master/LICENSE>
 * @link http://filesizejs.com
 * @module filesize
 * @version 2.0.0
 */
( function ( global ) {
"use strict";

var bit   = /b$/,
    bite  = /^B$/,
    radix = 10,
    right = /\.(.*)/,
    zero  = /^0$/;

/**
 * filesize
 *
 * @method filesize
 * @param  {Mixed}   arg        String, Int or Float to transform
 * @param  {Object}  descriptor [Optional] Flags
 * @return {String}             Readable file size String
 */
function filesize ( arg, descriptor ) {
	var result = "",
	    skip   = false,
	    i      = 6,
	    base, bits, neg, num, round, size, sizes, unix, spacer, suffix, z;

	if ( isNaN( arg ) ) {
		throw new Error( "Invalid arguments" );
	}

	descriptor = descriptor || {};
	bits       = ( descriptor.bits === true );
	unix       = ( descriptor.unix === true );
	base       = descriptor.base   !== undefined ? descriptor.base   : unix ? 2  : 10;
	round      = descriptor.round  !== undefined ? descriptor.round  : unix ? 1  : 2;
	spacer     = descriptor.spacer !== undefined ? descriptor.spacer : unix ? "" : " ";
	num        = Number( arg );
	neg        = ( num < 0 );

	// Flipping a negative number to determine the size
	if ( neg ) {
		num = -num;
	}

	// Zero is now a special case because bytes divide by 1
	if ( num === 0 ) {
		if ( unix ) {
			result = "0";
		}
		else {
			result = "0" + spacer + "B";
		}
	}
	else {
		sizes = options[base][bits ? "bits" : "bytes"];

		while ( i-- ) {
			size   = sizes[i][1];
			suffix = sizes[i][0];

			if ( num >= size ) {
				// Treating bytes as cardinal
				if ( bite.test( suffix ) ) {
					skip  = true;
					round = 0;
				}

				result = ( num / size ).toFixed( round );

				if ( !skip && unix ) {
					if ( bits && bit.test( suffix ) ) {
						suffix = suffix.toLowerCase();
					}

					suffix = suffix.charAt( 0 );
					z      = right.exec( result );

					if ( !bits && suffix === "k" ) {
						suffix = "K";
					}

					if ( z !== null && z[1] !== undefined && zero.test( z[1] ) ) {
						result = parseInt( result, radix );
					}

					result += spacer + suffix;
				}
				else if ( !unix ) {
					result += spacer + suffix;
				}

				break;
			}
		}
	}

	// Decorating a 'diff'
	if ( neg ) {
		result = "-" + result;
	}

	return result;
}

/**
 * Size options
 *
 * @type {Object}
 */
var options = {
	2 : {
		bits  : [["B", 1], ["kb", 128],  ["Mb", 131072],  ["Gb", 134217728],  ["Tb", 137438953472],  ["Pb", 140737488355328]],
		bytes : [["B", 1], ["kB", 1024], ["MB", 1048576], ["GB", 1073741824], ["TB", 1099511627776], ["PB", 1125899906842624]]
	},
	10 : {
		bits  : [["B", 1], ["kb", 125],  ["Mb", 125000],  ["Gb", 125000000],  ["Tb", 125000000000],  ["Pb", 125000000000000]],
		bytes : [["B", 1], ["kB", 1000], ["MB", 1000000], ["GB", 1000000000], ["TB", 1000000000000], ["PB", 1000000000000000]]
	}
};

// CommonJS, AMD, script tag
if ( typeof exports !== "undefined" ) {
	module.exports = filesize;
}
else if ( typeof define === "function" ) {
	define( function () {
		return filesize;
	} );
}
else {
	global.filesize = filesize;
}

} )( this );
