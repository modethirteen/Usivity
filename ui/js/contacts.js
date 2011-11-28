/*
	contacts.js - manage all contact interactions  
	
	FUNCTIONS
		-
		-
*/
$(document).ready( function() {
	
	// DELETE A CONTACT
	$(".contact_delete").live("submit", function() {
		alert("delete not implemented yet");
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

	
	
	
	// LOAD DATA FROM LIVECONTACT WHEN EMAIL IS ENTERED
	$(".contact_email").live("blur", function() {
		
		var input	= $(this);
		var email 	= input.val();
		
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
				success: function(json, textStatus, xhr) { 
					
					if(json.status == "200")
					{
						//TODO:  ADD FUNCTION TO CHECK IF OBJECT EXISTS BEFORE SETTING IT.
						
						console.log(json);
						// Demographics
						$(".contact_age").val(json.demographics.age);
						$(".contact_gender").val(json.demographics.gender);
						$(".contact_locationgeneral").val(json.demographics.locationGeneral);
						
						// Contact Info
						$(".contact_firstname").val(json.contactInfo.givenName);
						$(".contact_lastname").val(json.contactInfo.familyName);
						
						// Organizations
						
						// Photos
						var newele = $(document.createElement('img'));
						newele.attr("src",json.photos[0].url);
						$(".contact_new .contact_picture").html(newele);
					}
					else
					{
						error(input,"Sorry, we can't find any additional information about " + email + ", please add it manually for now.");	
					}
				}
			});
		}
	});
});


