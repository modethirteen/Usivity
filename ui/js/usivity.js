$.getScript("/js/startup.js", function(data, textStatus, jqxhr) {
	
	// Iteraction Libraries
	$.getScript("/js/utility.js");	
	$.getScript("/js/template.js");
	$.getScript("/js/dropdowns.js");
	$.getScript("/js/modal.js");
	$.getScript("/js/forms.js");
	$.getScript("/js/animations.js");
	$.getScript("/js/search.js");
	$.getScript("/js/error.js");
	$.getScript("/js/timeago.js");
	
	// API Libraries
	$.getScript("/js/connections.js");
	$.getScript("/js/messages.js");
	$.getScript("/js/subscriptions.js");
	$.getScript("/js/contacts.js");
	$.getScript("/js/loaddata.js");
	$.getScript("/js/stream.js");
	
	// Authentication Libraries
	$.getScript("/js/login.js");
	$.getScript("/js/base64.js");
	
	// Trigger the Application
	$.getScript("/js/authenticate.js");
});




