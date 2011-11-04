/*
	dropdown.js - show, hide messages in both the openstream and mystream.  Handle message interactions
	
	FUNCTIONS
		-
		-
*/


$(document).ready(function() {
	
	$(".drop").live("click", function() {
		return false;	
	});
	$(".drop").live("hover", function() {
		$(".down").hide();
		var src = $(this).attr("href");
		var id = $(this).attr("id");
		var datahtml 	= usivity[id].markup; // TODO:  CHANGE THE LOCATION OF WHERE DATA IS STORED IN DATA.JS to match this format
		var dataurl 	= usivity[id].url;
		var drop 		= $(this);
		var down		= $(this).next(".down");
				
		// GET MARKUP
		if (down.length == 0)
		{
			$.get(src, function(markup) {
				var html = preparedata(markup, datahtml, dataurl, function(html) {
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