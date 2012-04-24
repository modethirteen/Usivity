/*
	loaddata.js - Batch loads data into the data library
	
	FUNCTIONS
		- loadopenstream()
		- loadusercontacts()
*/

// TRIGGER ALL DATA LOADING EVENTS
function loaddata()
{
	 // LOAD THE OPENSTREAM
	 loadopenstream();
	 setInterval("newopenstreammessage()",app.messageinterval);   //TODO:  PUT A TIMER KILL IN HERE IF THE API IS NOT AVAILABLE
 	
 	 // LOAD CONTACTS
	 setTimeout("loadusercontacts()",1000);
	 
	 // LOAD USER PANEL
	 setTimeout("loaduserpanel()",1500);
}

// LOAD USER CREDENTIALS LABEL
function loaduserpanel()
{
	userparams = {
		"dream.out.format" 	: "jsonp",
		"dream.out.pre"	: cb()
	};
	var href = "/template/user_panel.htm";
	var objecturi = apiuri(api.current,userparams);
		
	$.get(href, function(templatehtml) {	
		template(templatehtml, objecturi, "null",function(html) {
			$(".header .menu").html(html);
			$(".header .menu").fadeIn();
		});
	});
}

// LOAD THE OPEN STREAM
function loadopenstream()
{
	var templateuri = "/template/message.htm"; // TODO:  PUT IN SETTINGS.jS
	

	// CALCULATE & SET TIMESTAMP (NOW)
	var now = new Date();
	timestamp 	= ISODateString(now);
	
	// CALCULATE & SET TIMEAGO (-messageloadspan)
	timeago = now.setSeconds(0,-app.messageloadspan);
	timeago 	= ISODateString(new Date(timeago));
	
	openstreamparams = {
		"stream" : "open",
		"dream.out.format" : "jsonp",
		"dream.out.pre": cb(),
		"start" : timeago,
		"limit" : app.messagelimit
	};
	var objecturi = apiuri(api.messages,openstreamparams);
	
 	$.get(templateuri, function(templatehtml) {
		template(templatehtml, objecturi, "messages_message", function(html) {

			$(".openstream .target tbody").append(html);
			$(".openstream").removeClass("loading");
			$(".openstream .target").fadeIn();
			
			// Process links
			$(".message_new").each( function() {
				var text = $(this).find(".message_text").html();
				var text = fixmessage($(this).find(".message_text").html());
				$(this).find(".message_text").html(text);
				$(this).removeClass("message_new");	
			});
			
			
			// Add timestamp row
			var timerow = '<tr class="time_row"><td colspan="9" >Between (<span class="timeago" title="' + timestamp + '"></span>) and (<span class="timeago" title="' + timeago + '"></span>)</td></tr>';
			$(".openstream .target tbody").prepend(timerow);
			jQuery(".timeago").timeago();
			
		});
	});	
}

// LOAD THE CONTACTS - DELAYED FOR 1 SECOND TO AVOID TIMING CONFLICTS WITH THE TEMPLATING ENGINE
function loadusercontacts()
{
	
	//TODO: PUT LOADING ICON ON MY CONTACTS
 	var templateuri = "/template/contact.htm";
 	contactparams = {
		"dream.out.format" : "jsonp",
		"dream.out.pre": cb()
	};
	var objecturi = apiuri(api.contacts,contactparams);
 	
 	$.get(templateuri, function(templatehtml) {
		template(templatehtml, objecturi, "contacts_contact", function(html) {
			$(".contacts .target tbody").html(html);
			$(".contacts").removeClass("loading");
			$(".contacts .target").fadeIn();
		});
	});
}










