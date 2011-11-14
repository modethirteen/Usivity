/*
	messages.js - show, hide messages in both the openstream and mystream.  Handle message interactions
	
	FUNCTIONS
		- Minimize/Maximize Message
		- Send a message
		- Delete a message
*/


$(document).ready(function() {
	// MINIMIZE/MAXIMIZE MESSAGE
	$(".message .minimize").live("click", function() {
		$(this).parents(".message").find(".message_text").slideToggle('fast', function() {
    
		});
		return false;
	});	
	
	// SEND A MESSAGE
	$(".message_send").live("submit", function() {
		var href	= $(this).attr("action");
		
		// TODO:  ADD RESPONSE FOR FAIL EVENT
		$.ajax({
			type: "POST",
			crossDomain:true,
			data: "sample message", //TODO:  PUT IN REAL MESSAGE STRING
			url: href,
			success: function(results)
			{
				console.log("worked");	
			},
			error: function(results) 
			{
				console.log("failed");	
			}
		});
		
		return false;	
	});
	
	
	// DELETE MESSAGE
	$(".message_delete").live("submit", function() {
		
		var id = $(this).attr("id");
		var apiaction = "?dream.in.verb=DELETE";
		//TODO: URL HREF FROM API, DON'T CONSTRUCT URI
		var apiurl = (usivity.apiroot.url + usivity.openstream.url + "/" + id + apiaction);
		
		$.ajax({
			type: "POST",
			crossDomain:true, 
			url: apiurl,
			success: function(results)
			{
				closeModal();	
			}
		});
		
		
		$("#" + id).parents(".parent").remove();
		
		return false;
	});
});

