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
	
	// FORMAT PHONE NUMBERS
	$(".contact_phonenumber").live("keyup", function() {
		var number = $(this).val().replace(/(\d{3})(\d{3})(\d{4})/, "($1)-$2-$3");	
		$(this).val(number);
	});
	
	
	// EDIT EXISTING CONTACTS  
	$(".contact_put").live("submit", function() {
		var form	= $(this);
		var id 		= $(this).attr("id");
		var uri		= (api.root + api.contacts);
		var href 	= $(this).attr("href");
		
		// PREP THE FORM
		process(form);
		clearstart(form);
		var data = get_contact_inputs(form);
		startset();
					
		updateparams = {
			"dream.in.verb" : "PUT",
			"dream.out.format" : "json"
		};
		
		var updateuri = apiuri(href,updateparams);
		
		console.log(data);
		
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
				loadusercontacts();
				success(form);
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
				console.log(xhr.status);
				console.log(thrownError);
			}  
		});	
		
		return false;		
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
