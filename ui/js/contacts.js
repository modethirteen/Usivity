/*
	contacts.js - manage all contact interactions  
	
	FUNCTIONS
		-
		-
*/
$(document).ready( function() {
	
	// DELETE A CONTACT
	$(".contact_delete").live("submit", function() {
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
	
	
	// POST A NEW CONTACT
	$(".contact_new").live("submit", function() {
		clearstart($(this)); // ONLY DO THIS IF THE MODAL IS GOING TO CLOSE
		return false;		
	});
	
// 	{
//     user:{
//         firstname:'foo',
//         lastname:'bar'
//     }
// 	}
	
	// LOAD DATA FROM LIVECONTACT WHEN EMAIL IS ENTERED
	$(".contact_email").live("blur", function() {
		
		var email = $(this).val();
		
		if (checkemail(email))
		{
			var fullcontactapi = 'https://api.fullcontact.com/v1/person.json?email=' + email + '&apiKey=e7ff635bfe5e9987&callback=callback';
			
			$.ajax({
				type: "GET",
				crossDomain:true, 
				url: fullcontactapi,
				dataType: 'jsonp',
				jsonp: false,
				jsonpCallback: 'callback',
				mimeType: 'application/json',
				contentType: 'application/json;',
				success: function(json)
				{
					console.log(json);
					$(".contact_firstname").val(json.contactInfo.givenName);
					$(".contact_lastname").val(json.contactInfo.familyName);
				}
			});
		}
	});
});


