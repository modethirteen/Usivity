/*
	messages.js - show, hide messages in both the openstream and mystream.  Handle message interactions
	
	FUNCTIONS
		- Send a message
		- Delete a message
*/


$(document).ready(function() {
	// SEND A MESSAGE
	$(".message_send").live("submit", function() {
		var uri		= $(this).attr("action");
		var message 	= $(this).find(".message_data").val();
		
		messageparams = {
			"dream.out.format" : "json"
		};
		
		var uri = apiuri(uri,messageparams);
		
		// TODO:  ADD RESPONSE FOR FAIL EVENT
		$.ajax({
			type: "POST",
			crossDomain:true,
			data: message, 
			url: uri,
			success: function(results)
			{
				loadusercontacts();
				closeModal();
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
		
		var href 	= $(this).attr("id");
		var id		= $(this).find(".id").val();
		
		deleteparams = {
			"dream.in.verb" : "DELETE"
		};
		
		var deleteuri = apiuri(href,deleteparams);
		
		$.ajax({
			type: "POST",
			crossDomain:true, 
			url: deleteuri,
			success: function(results)
			{		
				$("#" + id).slideUp();
				closeModal();	
			}
		});
		
		return false;
	});
});

