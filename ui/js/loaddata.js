/*
	loaddata.js - Batch loads data into the data library
	
	FUNCTIONS
		- loadopenstream()
		- loaduserstream()
		- loadusercontacts()
*/

$(document).ready(function() {
	
	// LOAD OPEN STREAM
	// TODO: ONLY TRY TO LOAD THE STREAMS IF THE USER IS AUTHENTICATED
	
 	$(window).load(function() {	
	 	// 0 second delay for loading my stream
	 	loadopenstream();
	 	setInterval("newopenstreammessage()",usivity.messageinterval.value);   //TODO:  PUT A TIMER KILL IN HERE IF THE API IS NOT AVAILABLE
 	});

 	// LOAD MY CONTACTS	
 	// TODO: ONLY TRY TO LOAD THE STREAMS IF THE USER IS AUTHENTICATED
 	
 	$(window).load(function() {	
	 	// 1 second delay for loading my contacts
	 	setTimeout("loadusercontacts();",1000);   //TODO:  PUT A TIMER KILL IN HERE IF THE API IS NOT AVAILABLE
 	});
 	
});

// LOAD THE OPEN STREAM
function loadopenstream()
{
	var templateuri = "/template/message.htm"; // TODO:  PUT IN SETTINGS.jS
	
	// Calculate $timeago = 120 minutes ago
	var now = new Date();
	var timeago = new Date().setDate(now.getDate()-7);
	var timeago = ISODateString(new Date(timeago));
	
	openstreamparams = {
		"stream" : "open",
		"dream.out.format" : "jsonp",
		"dream.out.pre": cb(),
		"start" : timeago,
		"limit" : 20
	};
	var objecturi = apiuri(usivity.openstream.url,openstreamparams);
	
 	$.get(templateuri, function(templatehtml) {
		template(templatehtml, objecturi, "messages_message", function(html) {
			
			$(".openstream .target tbody").append(html);
			$(".openstream").removeClass("loading");
			$(".openstream .target").fadeIn();
			$(".openstream .target tbody tr").addClass("inton");
			
			// Process links
			$(".message_new").each( function() {
				var text = $(this).find(".message_text").html();
				// var text = fixmessage($(this).find(".message_text").html());
				$(this).find(".message_text").html(text);
				$(this).removeClass("message_new");	
			});
			
			
			// Add timestamp row
			var now = new Date();
			var timestamp = ISODateString(now);
			var timeago = now.setMinutes ( now.getMinutes() - 120 );
			var timeago = ISODateString(new Date(timeago));
			var timerow = '<tr class="time_row"><td colspan="9" >Between (<span class="timeago" title="' + timestamp + '"></span>) and (<span class="timeago" title="' + timeago + '"></span>)</td></tr>';
			$(".openstream .target tbody").prepend(timerow);
			jQuery(".timeago").timeago();
			
		});
	});	
}

// LOAD THE CONTACTS - DELAYED FOR 4 SECOND TO AVOID TIMING CONFLICTS WITH THE TEMPLATING ENGINE
function loadusercontacts()
{
	
	//TODO: PUT LOADING ICON ON MY CONTACTS
 	var templateuri = "/template/contact.htm";
 	contactparams = {
		"dream.out.format" : "jsonp",
		"dream.out.pre": cb()
	};
	var objecturi = apiuri(usivity.contacts.url,contactparams);
 	
 	$.get(templateuri, function(templatehtml) {
		template(templatehtml, objecturi, "contacts_contact", function(html) {
			$(".contacts .target tbody").html(html);
			$(".contacts").removeClass("loading");
			$(".contacts .target").fadeIn();
		});
	});
}










