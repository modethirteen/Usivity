/*
	messages.js - show, hide messages in both the openstream and mystream.  Handle message interactions
	
	FUNCTIONS
		- Send a message
		- Delete a message
*/


$(document).ready(function() 
{
	
	// DELETE MESSAGE
	$(".message_delete").live("submit", function() {
		var href = $(this).attr("id");
		var id = $(this).find(".id").val();
		
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
				var deleterow = '<td colspan="9"><div>message deleted</div></td>';
				$("#" + id).addClass("delete_row");
				$("#" + id).html(deleterow);
				closeModal();	
			}
		});
		
		return false;
	});
	
		
	// SEND A MESSAGE
	$(".message_send, .message_send_inline").live("submit", function() {
		
		form = $(this);
		var uri	= form.attr("action");
		var message	= form.find(".message_data").val();
		var dcb = cb();
		var totalcountele = $(this).parents(".message_thread").find(".totalcount");
		var replyele = $(this).parents(".message_thread").find(".message_reply_inline");
		
		// ADD THE PROCESSING ICON
		process(form);
		
		messageparams = {
			"dream.out.format" 	: "json"
		};
	
		var uri = apiuri(uri,messageparams);
		
		$.ajax({
			type: "POST",
			crossDomain:true,
			data: message,
			url: uri,
			dataType: "json",
			mimeType: 'application/json',
			contentType: 'application/json',
			success: function(results)
			{
				// DISPLAY THE MESSAGE ON THE POPUP SCREEN	
				var src = "/template/message_thread.htm";
				var objecturi = results["@href"];
				
				messageparams = {
					"dream.out.format" 	: "jsonp",
					"dream.out.pre"	: cb()
				};
				var objecturi = apiuri(objecturi,messageparams);
				
				$.get(src, function(templatehtml) {		
					template(templatehtml, objecturi, "null",function(html) {
						form.parents(".message_thread").next(".message_thread_children").prepend(html);
						
						var messageinput = form.find(".message_data");
						
						messageinput.val("");
						messageinput.focus();
						
						// ADD THE COMPLETE/SUCCESS ICON
						success(form);

						// UPDATE THE NUMBER COUNTS // TODO: REPLACE THIS WITH A CALL TO THE API TO GET LATEST MESSAGES AND COUNTS						
						if (totalcountele.length >= 1)
						{
							var count = totalcountele.attr("value");
							var newcount = (Number(count)+1);
							totalcountele.attr("value",newcount);
							totalcountele.html("(" + newcount + ")");
						}
						else
						{
							var newele = $(document.createElement('span'));
							newele.addClass("totalcount");
							newele.attr("value","1");
							newele.html("(1)");
							replyele.append(newele);
						}
						
						// UPDATE THE .TIMEAGO ELEMENT
						jQuery(".timeago").timeago();
						
					});
				});
				
				
				// REFRESH THE CONTACT LIST
				loadusercontacts();
				
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
				console.log(xhr.status);
				console.log(thrownError);
				error(form);
			}  
		});
		
		return false;	
	});
	
	// SHOW MESSAGE REPLIES AND REPLY FORM
	$(".message_reply_inline").live("click", function() {
		
		link = $(this);
		var target = $(this).parents(".message_thread").next(".message_thread_children");
		var input = $(this).parents(".message_thread").find("textarea");
		
		// ADD CHARCTER COUNTER
		charactercount(input);
		
		// CHECK TO SEE IF REPLIES WERE ALREADY LOADED
		if (link.hasClass("loaded"))
		{
			link.parents(".message_thread").find(".message_send_inline").toggle();
			target.toggle();	
		}
		else
		{		
			// LOAD REPLIES FROM API
			var objecturi = $(this).attr("href");
			var src = "/template/message_thread.htm";
			
			
			messageparams = {
				"dream.out.format" : "jsonp",
				"dream.out.pre": cb(),
				"children": "flat"
			};
		
			var objecturi = apiuri(objecturi,messageparams);
			
			$.get(src, function(templatehtml) {		
				template(templatehtml, objecturi, "message_messages.children_message",function(html) {
					target.html(html);
					
					link.addClass("loaded");
					link.parents(".message_thread").find(".message_send_inline").show();
					target.show();
					
					// LOAD .TIMEAGO VALUES
					jQuery(".timeago").timeago();
				});
			});
		}

		return false;	
	});
});

