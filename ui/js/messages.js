/*
	messages.js - show, hide messages in both the openstream and mystream.  Handle message interactions
	
	FUNCTIONS
		- Send a message
		- Delete a message
*/


$(document).ready(function() {
	
	
	// SEND A MESSAGE
	$(".message_send, .message_send_inline").live("submit", function() {
		var uri			= $(this).attr("action");
		var message 	= $(this).find(".message_data").val();
		var dcb 		= cb();
		
		messageparams = {
			"dream.out.format" 	: "jsonp",
			"dream.out.pre"	: dcb
		};
	
		
		var objecturi = apiuri(uri,messageparams);
		
		$.ajax({
			type: "POST",
			crossDomain:true,
			data: message,
			dataType: 'jsonp',
			jsonpCallback: dcb,
			mimeType: 'application/json',
			contentType: 'application/json;',
			url: objecturi,
			success: function(results)
			{
				// DISPLAY THE MESSAGE ON THE POPUP SCREEN	
				var src = "/template/message_thread.htm";
				var objecturi = results.message["@href"];
				
				messageparams = {
					"dream.out.format" 	: "jsonp",
					"dream.out.pre"	: cb()
				};
				var objecturi = apiuri(objecturi,messageparams);
				
				$.get(src, function(templatehtml) {		
					template(templatehtml, objecturi, "null",function(html) {
						$(".message_threads").append(html);
					});
				});
				
				
				// REFRESH THE CONTACT LIST
				loadusercontacts();
				
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
				console.log(xhr.status);
				console.log(thrownError);
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

