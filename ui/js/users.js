/*
	users.js - manage users through the control panel
	
	FUNCTIONS
		-
		-
*/
$(document).ready( function() {
	
	// DELETE Users
	$(".users .delete").live("click", function() {
		var link	= $(this);
		var href	= link.attr("href");
		
		deleteparams = {
			"dream.in.verb" : "DELETE"
		};
		var uri = apiuri(href,deleteparams);
		$.ajax({
			type: "POST",
			crossDomain:true, 
			url: uri,
			success: function(results)
			{
				link.parents("tr").remove(); 		
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
			} 
		});
		
		return false;	
	});	
	
	
	// CREATE A NEW USER
	$(".users form").live("submit", function() {
		var form 		= $(this);
		var uri 		= form.attr("action");
		var username = $(this).find(".user_name");
		var password = $(this).find(".user_password");
		
		data = {
		    user:{
		        name: username.val(),
		        password : password.val()
		    }
		}
		
		userparams = {
			"dream.out.format" : "json"
		};
		
		var uri = apiuri(uri,userparams);
		
		$.ajax({
			type: "POST",
			crossDomain:true,
			data: JSON.stringify(data),
			url: uri,
			dataType: "json",
			mimeType: 'application/json',
			contentType: 'application/json',
			success: function(results)
			{
				var newele = $(document.createElement('tr'));
				newele.html('<td>' + results.name + '</td><td>' + results.role + '</td><td class="center"><a href="' + results['@href'] + '" class="delete">delete</a></td>');
				$(".users table tbody").append(newele);
				username.val("");
				password.val("");
				username.focus();			
			},
			error:function (xhr, ajaxOptions, thrownError){
				console.log(xhr.statusText);
			}   
		});
		
		return false;	
	});
		
});