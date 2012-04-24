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
	
	
	// ADD A NEW CONTACTS  
	$(".contact_post").live("submit", function() {
		
		var form	= $(this);
		var id 		= $(this).attr("id");
		var uri		= (api.root + api.contacts);
		
		clearstart(form);
		var data = get_contact_inputs(form);
		
		$.ajax({
			type: "POST",
			crossDomain:true,
			data: data,
			dataType: "json",
			mimeType: 'application/json',
			contentType: 'application/json',
			url: uri,
			success: function(results)
			{
				loadusercontacts();
				closeModal();	
			},
			error: function(results) 
			{
				console.log("failed");	
			}
		});	
		
		return false;		
	});		
	
	
	// EDIT EXISTING CONTACTS  
	$(".contact_put").live("submit", function() {
		
		
		var form	= $(this);
		var id 		= $(this).attr("id");
		var uri		= (api.root + api.contacts);
		var href 	= $(this).attr("href");
		
		clearstart(form);
		var data = get_contact_inputs(form);
					
		updateparams = {
			"dream.in.verb" : "PUT",
			"dream.out.format" : "json"
		};
		
		var updateuri = apiuri(href,updateparams);
		
		
		$.ajax({
			type: "POST",
			crossDomain:true,
			data: data,
			dataType: "json",
			mimeType: 'application/json',
			contentType: 'application/json',
			url: updateuri,
			success: function(results)
			{
				console.log(results);
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
				console.log(xhr.status);
				console.log(thrownError);
			}  
		});	
		
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

function get_contact_inputs(form)
{
	data = {
		contact : {
			firstname:  		form.find(".contact_firstname").val(),
			lastname:	  		form.find(".contact_lastname").val(),
			"uri.avatar":		form.find(".contact_picture img").attr("src"),
			age:  				form.find(".contact_age").val(),
			gender:  			form.find(".contact_gender").val(),
			"location":  		form.find(".contact_locationgeneral").val(),
			email:  			form.find(".contact_email").val(),
			phone:  			form.find(".contact_phonenumber").val(),
			fax:  				form.find(".contact_faxnumber").val(),
			address:  			form.find(".contact_address").val(),
			city:	  			form.find(".contact_city").val(),
			state:  			form.find(".contact_state").val(),
			zip:	  			form.find(".contact_zip").val(),
			"identity.twitter": 	form.find(".contact_twitter").val(),
			"identity.facebook": 	form.find(".contact_facebook").val(),
			"identity.linkedin": 	form.find(".contact_linkedin").val(),
			"identity.google": 		form.find(".contact_google").val(),
			company : {
				name:				form.find(".company_name").val(),
				phone:				form.find(".company_phonenumber").val(),
				address:			form.find(".company_address").val(),
				city:				form.find(".company_city").val(),
				state:				form.find(".company_state").val(),
				zip:				form.find(".company_zip").val(),
				industry:			form.find(".company_industry").val(),
				revenue:			form.find(".company_revenue").val(),
				competitors:		form.find(".company_competitors").val()
			}
		}	
	}	
	
	return JSON.stringify(data);
}

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