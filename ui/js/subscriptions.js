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
		var href	= link.attr("href");
		
		deleteparams = {
			"dream.in.verb" : "DELETE"
		};
		var uri = apiuri(href,deleteparams);
		$.ajax({
			type: "POST",
			crossDomain:true, 
			url: uri,
			success: function(results)
			{
				link.parents("li").remove(); 		
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
			} 
		});
		
		return false;	
	});	
	
	
	/*POST SUBSCRIPTIONS*/
	$(".subscriptionsform form").live("submit", function() {
		var form 		= $(this);
		var url 		= form.attr("action");
		var input		= form.find(".constraints");
		var constraints = input.val();
		
		subscriptionparams = {
			"constraints" : constraints
		};
		
		var uri = apiuri(url, subscriptionparams);
		$.ajax({
			type: "POST",
			crossDomain:true, 
			url: uri,
			success: function(results)
			{
				// TODO: GET RESULTS FROM API (TALK TO ANDY) - CAN'T HAVE DELETE FOR NEWLY ADDED SUBSCRIPTIONS
				console.log("success");
				input.val("");
				input.focus("");
				var newele = $(document.createElement('li'));
				newele.html('<span>' + constraints + '</span>');
				$(".subscriptionsform ul").append(newele);
				
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
			}   
		});
		
		return false;	
	});
});