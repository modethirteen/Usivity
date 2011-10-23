/*
	subscriptions.js - manage all stream subscriptions.  Example - words that are searched on twitter
	
	FUNCTIONS
		- 
		-
*/

$(document).ready(function() {
	
	// DELETE SUBSCRIPTION
	$(".subscriptionsform .delete").live("click", function() {
		var link	= $(this);
		var id 		= link.attr("id");
		
		var apiaction = "?dream.in.verb=DELETE";
		var apiurl = (usivity.apiroot.url + usivity.subtwitter.url + "/" + id + apiaction);
		
		// TODO:  ADD RESPONSE FOR FAIL EVENT
		$.ajax({
			type: "POST",
			crossDomain:true, 
			url: apiurl,
			success: function(results)
			{
				link.parents("li").remove(); 		
			}
		});
		
		return false;	
	});	
	
	
	/*POST SUBSCRIPTIONS*/
	$(".subscriptionsform form").live("submit", function() {
		var form 	= $(this);
		var url 	= form.attr("action");
		var input	= form.find(".constraints");
		
		var constraints = input.val();
		var apiurl = (usivity.apiroot.url + url + "?constraints=" + constraints);  //TODO: Stop using url as variable
		
		$.ajax({
			type: "POST",
			data: constraints,
			crossDomain:true, 
			url: apiurl,
			success: function(results)
			{
				// TODO: GET RESULTS FROM API (TALK TO ANDY) - CAN'T HAVE DELETE FOR NEWLY ADDED SUBSCRIPTIONS
				input.val("");
				input.focus("");
				var newele = $(document.createElement('li'));
				newele.html('<span>' + constraints + '</span>');
				$(".subscriptionsform ul").append(newele);
			}
		});
		
		return false;	
	});
});