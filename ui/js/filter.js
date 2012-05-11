/*
	dropdown.js - show, hide messages in both the openstream and mystream.  Handle message interactions
	
	FUNCTIONS
		- No-click
		- Show dropdown
		- Hide dropdown
		- positiondrop()
*/


$(document).ready(function() {
	
	// TOGGLE THE FILTER TO SHOW/HIDE
	$(".tools_filters, .filter .close").click( function() {
		loadsubscriptions();
		$(".filter td").toggle();
		return false;
	});
	
	// FILTER BY SOURCES
	$(".filter_source input").live("click", function() {
		
		var type = $(this).attr("value");
		var checked = $(this).attr("checked");
		var typeclass = (".message_source_" + type);

		if (checked == "checked")
		{
			$(typeclass).show();
		}
		else
		{
			$(typeclass).hide();
		}
		
		var filtercount = $(".filter_source input:not(:checked)").length;
		
		if (filtercount == 0)
		{
			$(".time_row").show();	
		}
		else
		{
			$(".time_row").hide();	
		}
		
		
		
	});
});

// LOAD SUBSCRIPTIONS LIST - PLACE INTO FILTER
function loadsubscriptions()
{
	var src = "/template/filter_subscriptions.htm";
	var objecturi = apiuri("/api/1/subscriptions",api.params);
	
	$.get(src, function(templatehtml) {		
		template(templatehtml, objecturi, "null",function(html) {
			$(".subscriptions_target").html(html);
			return true;
		});
	});
}