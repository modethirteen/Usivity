/*
	dropdown.js - show, hide messages in both the openstream and mystream.  Handle message interactions
	
	FUNCTIONS
		- No-click
		- Show dropdown
		- Hide dropdown
		- positiondrop()
*/


$(document).ready(function() {
	
	// Filter
	$(".col_filter").live("click", function() {
		var ftarget = $(this);
		var fdrop	= ftarget.find(".filter_dropdown");
		var templateuri = $(this).find(".filter").attr("href");
		
		if ($(fdrop).length == 0)
		{
			$.get(templateuri, function(templatehtml) {
				ftarget.append(templatehtml);
				ftarget.find(".filter_dropdown").show();
			});
		}
		$(".filter_dropdown").hide();
		$(fdrop).show();	
	
	});
	
	// No-Click
	$(".col_filter").live("click", function() {
		return false;	
	});
	
	// Hide Dropdown
	$("body").click( function() {
		$(".filter_dropdown").hide();
	});

});

function positiondrop(drop,down)
{
	var droppos			= drop.position();
	var dropleft 		= droppos.left;
	var dropwidth		= drop.outerWidth();
	var downwidth		= down.outerWidth();
	var downleft 		= (dropleft - ((downwidth - dropwidth) / 2));
	
	down.css("left",downleft + "px");
	down.show();	
}