/*
	stream.js - stream in content in bulk.  Batch load data into memory and slowly add messages to the stream
	
	FUNCTIONS
		- outputmessages(params)			Add messages to the openstream one at a time
		- fillqueue(uri,params,callback)	Pull data from the API and load it into memory
*/
function newopenstreammessage()
{
	var templateuri = "/template/message_open.htm"; // TODO:  PUT IN SETTINGS.jS
	openstreamparams = {
		"stream" : "open",
		"dream.out.format" : "jsonp",
		"dream.out.pre": cb(),
		"limit": "10"
	};
	var objecturi = apiuri(usivity.openstream.url,openstreamparams);
 	
 	$.get(templateuri, function(templatehtml) {
		template(templatehtml, objecturi, "null", function(html) {
			
			// Load The Date into the Open Stream
			$(".openstream .target tbody").prepend(html);
			
			// Process links
			$(".message_new").each( function() {
				var text = $(this).find(".message_text").html();
				// var text = fixmessage($(this).find(".message_text").html());
				$(this).find(".message_text").html(text);
				$(this).removeClass("message_new");	
			});
			
			// Add the timestamp row
			var timestamp = ISODateString(new Date());
			var timerow = '<tr class="time_row"><td class="timeago" colspan="9" title="' + timestamp + '"></td></tr>';
			$(".openstream .target tbody").prepend(timerow);
			jQuery(".timeago").timeago();
			
		});
	});	
}