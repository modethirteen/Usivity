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
		"limit": "1"
	};
	var objecturi = apiuri(usivity.openstream.url,openstreamparams);
 	
 	$.get(templateuri, function(templatehtml) {
		template(templatehtml, objecturi, "null", function(html) {
			var newele = $(document.createElement('div'));
			newele.html(html);
			newele.css("display","none");
			$(".openstream .target").prepend(newele);
			newele.slideDown();
			$(".openstream .target>div:last").remove();
		});
	});	
}