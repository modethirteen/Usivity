/*
	dropdown.js - show, hide messages in both the openstream and mystream.  Handle message interactions
	
	FUNCTIONS
		- No-click
		- Show dropdown
		- Hide dropdown
		- positiondrop()
*/


$(document).ready(function() {
	
	$(".tools_filters, .filter .close").click( function() {
		$(".filter td").toggle();
		return false;
	});
});