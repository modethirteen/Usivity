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
				loaduserstream();
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
	
	
	// View Message Thread
	$(".message_view_thread").live("click", function() {
	
		var parent		= $(this).parents(".message");
		var href	 	= $(this).attr("id");
		var templateuri	= "/template/message_thread.htm";
		
		threadparams = {
			"dream.out.format" : "jsonp",
			"dream.out.pre" : cb(),
			"include" : "children"
		};
		
		var objecturi = apiuri(href,threadparams);
		
		$.get(templateuri, function(templatehtml) {
			template(templatehtml, objecturi, null, function(html) {
				var newele = $(document.createElement('div'));
				newele.html(html);
				newele.css("display","none");
				parent.append(newele);
				newele.show();
			});
		});	
		return false;	
	});
});

