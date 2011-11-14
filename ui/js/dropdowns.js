/*
	dropdown.js - show, hide messages in both the openstream and mystream.  Handle message interactions
	
	FUNCTIONS
		- No-click
		- Show dropdown
		- Hide dropdown
		- positiondrop()
*/


$(document).ready(function() {
	
	// No-Click
	$(".drop").live("click", function() {
		return false;	
	});
	
	// Show dropdown
	$(".drop").live("hover", function() {
		$(".down").hide();
		var templateuri = $(this).attr("href");
		var id = $(this).attr("id");
		
		dropparams = {
			"dream.out.format" : "jsonp",
			"dream.out.pre": "callback"
		};
		var objecturi = apiuri(usivity[id].url,dropparams); //TODO:  clear up the first param.  Not sure if this looks good
		var drop 		= $(this);
		var down		= $(this).next(".down");
		
		// GET MARKUP
		if (down.length == 0)
		{
			$.get(templateuri, function(templatehtml) {
				preparedata(templatehtml, objecturi, function(html) {  // TODO:  CHANGE THE INPUT PARAMETERS FOR PREPAREDATA()
					var down = $(document.createElement('div'));
					down.html(html);
					down.addClass('down');
					drop.after(down);		

					positiondrop(drop,down);
				});
			});
		}
		else
		{
			positiondrop(drop,down);
		}
		
	});
	
	
	// Hide Dropdown
	$("body").click( function() {
		$(".down").hide();
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