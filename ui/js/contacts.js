/*
	contacts.js - manage all contact interactions  
	
	FUNCTIONS
		-
		-
*/
$(document).ready( function() {
	
	// DELETE A CONTACT
	$(".contact_delete").live("click", function() {
		var href 	= $(this).attr("href");
		var id		= $(this).attr("id");
		
		deleteparams = {
			"dream.in.verb" : "DELETE"
		};
		
		console.log(href);
		console.log(id);
		
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
	
	
	// MANAGE CONTACTS  
	$(".contact_post").live("submit", function() {
		
		var form	= $(this);
		var id 		= $(this).attr("id");
		var uri		= (usivity.apiroot.url + usivity.contacts.url);
		
		clearstart(form);
		
		data = {
			contact : {
				firstname:  	$(this).find(".contact_firstname").val(),
				lastname:	  	$(this).find(".contact_lastname").val(),
				"uri.avatar":		$(this).find(".contact_picture img").attr("src"),
				age:  			$(this).find(".contact_age").val(),
				gender:  		$(this).find(".contact_gender").val(),
				"location":  		$(this).find(".contact_locationgeneral").val(),
				email:  		$(this).find(".contact_email").val(),
				phone:  		$(this).find(".contact_phonenumber").val(),
				fax:  			$(this).find(".contact_faxnumber").val(),
				address:  		$(this).find(".contact_address").val(),
				city:	  		$(this).find(".contact_city").val(),
				state:  		$(this).find(".contact_state").val(),
				zip:	  		$(this).find(".contact_zip").val(),
				"identity.twitter": 	$(this).find(".contact_twitter").val(),
				"identity.facebook": 	$(this).find(".contact_facebook").val(),
				"identity.linkedin": 	$(this).find(".contact_linkedin").val(),
				"identity.google": 	$(this).find(".contact_google").val()
			}	
		}

		// ADD NEW CONTACT - POST
		if (!id || id == "")
		{
			console.log("new contact");
			// TODO:  ADD RESPONSE FOR FAIL EVENT
			$.ajax({
				type: "POST",
				crossDomain:true,
				data: JSON.stringify(data), 
				url: uri,
				success: function(results)
				{
					console.log("success");
					loadusercontacts();
					closeModal();	
				},
				error: function(results) 
				{
					console.log("failed");	
				}
			});	
		}			
		
		// EDIT EXISTING CONTACT - PUT
		if (id != "")
		{
			console.log("existing contact");	
		}
			
		return false;		
	});
	
	// LOAD DATA FROM LIVECONTACT WHEN EMAIL IS ENTERED
	$(".contact_email").live("blur", function() {
		var email = $(this).val();
		var form	= $(this).parents("form");
		if (checkemail(email))
		{
			var api = 'https://api.fullcontact.com/v2/person.json?email=' + email + '&apiKey=e7ff635bfe5e9987&callback=callback';
			resetstart(form);
			$(this).val(email);
			full_contact($(this),api);
		}
	});
});

function set_contact(selector, object)
{
	if($(object))
	{
		$(selector).val(object);
	}
	return false;
}

function full_contact(input, api)
{
	var form		= input.parents("form");
	var inputval 	= input.val();
	
		
	$.ajax({
		type: "GET",
		crossDomain:true, 
		url: api,
		dataType: 'jsonp',
		jsonp: false,
		jsonpCallback: 'callback',
		mimeType: 'application/json',
		contentType: 'application/json;',
		success: function(json, textStatus, xhr) { 
			
			if(json.status == "200")
			{
				// DEMOGRAPHICS
				set_contact(".contact_age",json.demographics.age);
				set_contact(".contact_gender",json.demographics.gender);
				set_contact(".contact_locationgeneral",json.demographics.locationGeneral);
				
				// CONTACT INFORMATION
				set_contact(".contact_firstname",json.contactInfo.givenName);
				set_contact(".contact_lastname",json.contactInfo.familyName);
				
				// COMPANY INFORMATION
				var company = {};
				for (var i in json.organizations)
				{
					var name = json.organizations[i].name;
					var isprimary = json.organizations[i]["isPrimary"]; 
					
					if (isprimary)
					{
						var company = json.organizations[i];
					}
				}
				if (company.name) set_contact(".company_name",company.name);
				
				// SOCIAL PROFILES
				var profiles = {};
				for (var i in json.socialProfiles)
				{
					var typeName = json.socialProfiles[i].typeName;
					profiles[typeName] = json.socialProfiles[i];
				}
				
				if (profiles.Linkedin) set_contact(".contact_linkedin",profiles.Linkedin.username);
				if (profiles.Facebook) set_contact(".contact_facebook",profiles.Facebook.username);
				if (profiles.Twitter) set_contact(".contact_twitter",profiles.Twitter.username);
				if (profiles["Google Plus"]) set_contact(".contact_google",profiles["Google Plus"].username);
				
				
				// DETERMINE THE PROFILE PICTURE
				var photos = {};
				for (var i in json.photos)
				{
					var typeName = json.photos[i].typeName;
					photos[typeName] = json.photos[i];	
				}
				
				if (photos.Facebook)
				{
					var photo = photos.Facebook;
				}
				else if (photos.Twitter)
				{
					var photo = photos.Twitter;
				}
				else if (photos.Linkedin)
				{
					var photo = photos.Linkedin;
				}
				
				// SET OR REMOVE THE PHOTO
				if (photo && photo.url)
				{
					var newele = $(document.createElement('img'));
					newele.attr("src",photo.url);
					form.find(".contact_picture").html(newele);
				}
				else
				{
					form.find(".contact_picture img").remove();
				}
				
				// UPDATE FIELD STATUS FOR STARTER FIELDS
				updatestart(form);
			}
			else
			{
				//error(input,"Sorry, we can't find any additional information about " + email + ", please add it manually for now.");	
			}
		}
	});
}