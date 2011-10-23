/*
	connections.js - handle all connections to SOURCES such as twitter, facebook, email, etc...
	
	FUNCTIONS
		- GET
		- POST
		- PUT
		- DELETE
		
	CUSTOM FUNCTIONS
		- LOAD AUTHENTICATION
*/
$(document).ready(function() {
	
	
	/*GET*/
	
	
	/*LOAD AUTHENTICATION*/
	$(".connections a.connection").live("click", function() {
		var width = 500;
    	var height = 500;
    	var left = parseInt((screen.availWidth/2) - (width/2));
    	var top = parseInt((screen.availHeight/2) - (height/2));
		var url = $(this).attr('href');
		
		/*Get Auth Token From Usivity API*/
		
		
		window.open(url,'mywin','left=' + left + ',top=' + top + ',width=' + width + ',height=' + height + ',toolbar=1,resizable=0');
		return false;	
	});
	
});