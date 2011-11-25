/*
	modal.js - handle all popup modal dialogs.  responsible for loading data, positioning and resizing modals  
	
	FUNCTIONS
		-
		-
*/
$(document).ready( function() {
	
	$(window).resize(function() {
		resizemodal();
	});
	
	// LOAD MODAL MARKUP
	$.get('/template/modal.htm', function(data) {
		$("body").append(data);
		resizemodal();
	});
	
	// CLOSE MODAL WINDOW
	$('.modal .close, .modal .cancel').live("click", function() {
		closeModal();
		return false;
	});
	
	// "NORMAL" POPUP LINK
	$(".popup, .dialog").live("click", function() {
		var link		= $(this);
		var src 		= link.attr("href");
		var objecturi	= link.attr("id");
		var type	= "";
	
		if (link.hasClass("dialog"))
		{
			var type = "modalsmall";
		}
		
		// TODO:  GET RID OF THIS IF STATEMENT
		if (objecturi)
		{		
			var objecturi = apiuri(objecturi,usivity.apiformat.value);
			$.get(src, function(templatehtml) {		
				template(templatehtml, objecturi, "null",function(html) {
					$(".modal .target").html(html);
					showmodal(type);	
				});
			});
		}
		else
		{
			// LOAD PAGES THAT DON'T NEED TO HAVE DATA FROM AN API
			$.get(src, function(data) {		
				$(".modal .target").html(data);
				showmodal(type);	
			});
		}
		return false;
	});
	
	// MODAL LARGE - POPUP FOR EXTERNAL LINKS
	$('.external').live("click", function() {
		$(".modal").addClass("modallarge");
		var src = $(this).attr("href");
		
		$.get("/template/external.htm", function(data) {
			$(".modal .target").html(data);
			$(".modal iframe").attr("src",src);
			showmodal();
		});
		return false;
	});
});

function closeModal() {
	$(".modal").fadeOut('fast', function() {
		$(".modal_bg").fadeOut('fast');
		$(".modal").removeClass("modalsmall"); // TODO: PUT THESE CLASSES IN SETTINGS AS AN ARRAY
		$(".modal").removeClass("modallarge"); // TODO: PUT THESE CLASSES IN SETTINGS AS AN ARRAY
	});	
}

function showmodal(type) {
	// Apply Type
	$(".modal").addClass(type);
	
	// Replace Title
	var title = $(".modal h1").html();
	$(".modal h2").html(title);
	$(".modal h1").remove();
	
	// Wrap Textareas with trap
	$("textarea").wrap('<div class="twrap"></div>');
	
	// SETUP "START" FIELDS
	startset();
	
	// SETUP THE RESPONSE (ERROR/SUCCESS) BOXES
	setupresponse();
	
	// Show Modal
	$(".modal_bg").fadeIn();
	$(".modal").fadeIn();
	
	// Resize Modal
	resizemodal();
}

function resizemodal() {
	// Dimensions of Body	
	var bheight = $(window).height();
	var bwidth = $(window).width();
	
	// CALCULATE HEIGHT OF MODAL TITLE
	var theight = $(".modal .title").outerHeight();
	
	// Dimensions of Modal
	var mheight = $(".modal").outerHeight();  // Outer height of modal
	var miheight = $(".modal").css('height').replace("px","");// Inner height of modal
	var mwidth = $(".modal").outerWidth();
	
	// Dimensions of Target
	var theight = (miheight - theight - 5);
	
	// Location of Modal
	var mtop = ((bheight - mheight)/2);
	var mleft = ((bwidth - mwidth)/2);
	
	// Apply Dimensions
	$(".modal_bg").height(bheight);
	$(".modal").css("top",mtop);
	$(".modal").css("left",mleft);	
}
