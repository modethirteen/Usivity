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
	$(".popup").live("click", function() {
		var src = $(this).attr("href");
		var id = $(this).attr("id");
		var datastore = openmessages.data[id];
		
		$.get(src, function(data) {
			var html = maketemplate(data, datastore);
			$(".modal .target").html(html);
			showmodal();
		});
		return false;
	});
	
	// MODAL DIALOG - POPUP FOR YES/NO DIALOGS
	// TODO - MERGE FUNCTION WITH (".popup")WITH A DOUBLE SELECTOR
	$(".dialog").live("click", function() {
		var src = $(this).attr("href");
		var id = $(this).attr("id");
		var datastore = openmessages.data[id];
		$(".modal").addClass("modaldialog");
		
		$.get(src, function(data) {
			var html = maketemplate(data, datastore);
			$(".modal .target").html(html);
			showmodal();
		});
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
		$(".modal").removeClass("modaldialog");
		$(".modal").removeClass("modallarge");
	});	
}

function showmodal() {
	
	// Replace Title
	var title = $(".modal h1").html();
	$(".modal h2").html(title);
	$(".modal h1").remove();
	
	// Wrap Textareas with trap
	$("textarea").wrap('<div class="twrap"></div>');
	
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
