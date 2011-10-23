/*
	messages.js - show, hide messages in both the openstream and mystream.  Handle message interactions
	
	FUNCTIONS
		-
		-
*/


$(document).ready(function() {
	/*MINIMIZE/MAXIMIZE MESSAGE*/
	$(".message .minimize").live("click", function() {
		$(this).parents(".message").find(".message_text").slideToggle('fast', function() {
    
		});
		return false;
	});	
	
	/*LOAD THE OPEN STREAM*/
	$(window).load(function() {
		//TODO:  CHANGE TO NEW OBJECT NAME:USIVITY
		fillmessagequeue(usivity.openstream.url,function(x) {
			loadmessage();
			setInterval("loadmessage()",usivity.messageinterval.value);
		});
	});
	
	/*DELETE MESSAGE*/
	$(".message_delete").live("submit", function() {
		
		var id = $(this).attr("id");
		var apiaction = "?dream.in.verb=DELETE";
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





//TODO:  RENAME TO SOMETHING LIKE OUTPUT MESSAGES OR EMPTY MESSAGE QUEUE
function loadmessage() 
{
	// GET LAST ID FROM ARRAY
	var id = usivity.ids.pop();  
	var data = usivity[id]; 
	
	var geturl 	= usivity.openmessages.temp;  //TODO: STORE VALUE IN A BETTER PLACE.  RENAME
	var wrap	= usivity.openmessages.wrap;  //TODO:  STORE VALUE IN A BETTER PLACE.  RENAME
	
	if (id)
	{
		$.get(geturl, function(markup)
		{

			var html = preparedata(markup, data);
			var newele = $(document.createElement('div'));
			newele.addClass("parent");
			newele.html(html);
			newele.css("display","none");
			$(wrap).find(".target").prepend(newele);
			newele.slideDown();
			//TODO:  ADD DELETE FOR OBJECTS - REMOVE USED OBJECT FROM DATA LIBRARY ONLY WHEN DONE
		});
	}
	
	// Refresh the datastore from the API
	if (usivity.ids.length == 3) // TRY TO GET RID OF .LENGTH, CAUSED JS PROBLEMS
	{
		fillmessagequeue(usivity.openstream.url);
	}
	
	// TODO:  Empty the stream for limit
	
}