/*
	stream.js - stream in content in bulk.  Batch load data into memory and slowly add messages to the stream
	
	FUNCTIONS
		- outputmessages(params)			Add messages to the openstream one at a time
		- fillqueue(uri,params,callback)	Pull data from the API and load it into memory
*/
function newopenstreammessage()
{
	var templateuri = "/template/message.htm"; // TODO:  PUT IN SETTINGS.jS
	
	// DETERMINE TIMES
	var now = new Date();
	timestamp = ISODateString(now);
	timeago = new Date().setTime(now.getDate() - app.messagedelay);
	timeago = ISODateString(new Date(timeago));
	
	openstreamparams = {
		"stream" : "open",
		"dream.out.format" : "jsonp",
		"dream.out.pre": cb(),
		"limit": "10",
		"start" : timeago,
		"limit" : 100
	};
	var objecturi = apiuri(api.messages,openstreamparams);
 	
 	$.get(templateuri, function(templatehtml) {
		template(templatehtml, objecturi, "null", function(html) {

			// COUNT THE MESSAGES IN THE OPENSTREAM
			
						
			// LOAD THE DATA INTO THE OPEN STREAM
			$(".openstream .target tbody").prepend(html);
			
			// PROCESS LINKS
			$(".message_new").each( function() {
				var text = $(this).find(".message_text").html();
				var text = fixmessage($(this).find(".message_text").html());
				$(this).find(".message_text").html(text);
				$(this).removeClass("message_new");	
			});
			
			// ADD THE TIMESTAMP
			if ($(".message_new").length > 0)
			{
				// Add the timestamp row
				var timerow = '<tr class="time_row"><td class="timeago" colspan="9" title="' + timeago + '"></td></tr>';
				$(".openstream .target tbody").prepend(timerow);
				jQuery(".timeago").timeago();
			}			
		});
	});	
}