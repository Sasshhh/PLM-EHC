/* 
 * Copyright 2016 materialDesignAdmin 
 */

$(document).ready(function() {
    "use strict";
    
	// ini the first product row in the table
	invoiceRow();
    invoiceRow();

	function invoiceRow() {
		var template = Handlebars.compile($('#invoiceRow').html() );        
        $('.create-invoice-row').append( template );
        setTimeout(function(){
            $('.invoiceRow').addClass('open');
        }, 100);
	}

	// Add a new product row to the table
  	$('#add-row').click(function () {
  		var newRow = invoiceRow();	    
	    $('#newInvoiceTable').append(newRow);
        $('#newInvoiceTable').responsiveTable();        
	});

    $('#newInvoiceTable').on('keyup', function() {
        // only recalculate when the user is done typing
        setTimeout(function(){
             calculateTotals();
         }, 500);
    });

    $('.row-tax-rate').on('change', function() {
        calculateTotals();
    });

    $('.discountTotalDisplay').hide();
    $('.shippingCostDisplay').hide();

    function calculateTotals(){
    
        // get all of the various input types from the rows 
        // each will be any array of elements

        var discountValue =  $('#discountValue');
        var discountType = $('#discountType').val();
        var dta = $('#discountTotalAmount');
        var shippingCost = $('#shippingCost')        
        var quantityElements = $('#newInvoiceTable tbody > tr');
        var priceElements = $('.row-price');
        var taxElements = $('.row-tax');
        var taxPercentageElements =$('.row-tax-rate');
        var totalElements = $('.row-total');
            
        // get the bottom table elements
        var subTotalElement =$('#subTotal');
        var discountTotalElement =$('#discountTotal');
        var shippingTotalElement = $('#shippingTotal');
        var totalTaxElement =$('#taxTotal');
        var grandTotalElement =$('#grandTotal');

        var subTotal = 0;
        var discountTotal = 0;
        var shippingTotal = 0;
        var taxTotal = 0;
        var grandTotal = 0;

        $(quantityElements).each(function(i,e){
            
            // get all the elements for the current row
            var nameElement = $('.row-name:eq(' + i + ')');
            var quantityElement = $('.row-quantity:eq(' + i + ')');
            var taxElement = $('.row-tax:eq(' + i + ')');
            var taxPercentageElement = $('.row-tax-rate:eq(' + i + ')');
            var priceElement = $('.row-price:eq(' + i + ')');
            var totalElement = $('.row-total:eq(' + i + ')');

            // get the needed values from this row
            var qty = quantityElement.val().trim().replace(/[^0-9$.,]/g, ''); // filter out non digits like letters
                //qty = qty == '' ? 0 : qty; // if blank default to 0
                quantityElement.val(qty); // set the value back, in case we had to remove something
            var price = priceElement.val().trim().replace(/[^0-9$.,]/g, '');
                //price = price == '' ? 0 : price; 
                priceElement.val(price);
            var taxPercentage = taxPercentageElement.val().trim().replace(/[^0-9$.,]/g, '');
                //taxPercentage = taxPercentage == '' ? 0 : taxPercentage;
                taxPercentageElement.val(taxPercentage+' %'); // remove this line if using a select

       
            // get/set row tax and total
            // also add to our totals for later
            var rowPrice = (price * 1000) * qty
                subTotal = subTotal + rowPrice;
            var tax = (taxPercentage / 100) * rowPrice;
                taxElement.val((tax / 1000).toFixed(2));
                taxTotal = taxTotal + tax;
            var total =   rowPrice
                totalElement.val((total / 1000).toFixed(2));
                grandTotal = grandTotal + total + tax;

        });
        $('.discountTotalDisplay').hide();
        $('.shippingCostDisplay').hide();
        
        var discount = discountValue.val().trim().replace(/[^0-9$.,]/g, '');
        //discount = discount == '' ? 0 : discount;
        discountValue.val(discount);

        var shipping = shippingCost.val().trim().replace(/[^0-9$.,]/g, '');
        shippingCost.val(shipping);
        shippingTotal = shipping;

        if (discount.length && !shipping.length) {
            $('.discountTotalDisplay').show();          

            if (discountType === 'percent') {            
                discountTotal = discount * (subTotal / 1000);
                dta.text('Discount ' + discount + ' %');
                discountTotalElement.text((discountTotal / 100).toFixed(2));
                grandTotal= (grandTotal / 1000);
                grandTotalElement.text((grandTotal - (discountTotal / 100)).toFixed(2));
            } else {
                discountTotal = discount;           
                dta.text('Discount');
                discountTotalElement.text((discountTotal / 1).toFixed(2));
                grandTotal= (grandTotal / 1000);
                grandTotalElement.text((grandTotal - discountTotal / 1).toFixed(2));
            }

        } else {
            grandTotalElement.text((grandTotal / 1000).toFixed(2)); 
        }        

        if (shipping.length && !discount.length) {            
            $('.shippingCostDisplay').show();
            shippingTotalElement.text((shippingTotal / 1).toFixed(2));
            grandTotal = (grandTotal / 1000);
            grandTotalElement.text((grandTotal + shippingTotal / 1).toFixed(2));
        }

        if (discount.length && shipping.length) {
            $('.discountTotalDisplay').show();
            $('.shippingCostDisplay').show();

            if (discountType === 'percent') {
                shippingTotalElement.text((shippingTotal / 1).toFixed(2));            
                discountTotal = discount * (subTotal / 1000);
                dta.text('Discount ' + discount + ' %');
                discountTotalElement.text((discountTotal / 100).toFixed(2));
                grandTotal= ((grandTotal / 1000) + parseFloat(shippingTotal));                
                grandTotalElement.text((grandTotal  - (discountTotal / 100)).toFixed(2));
            } else {
                shippingTotalElement.text((shippingTotal / 1).toFixed(2));
                discountTotal = discount;           
                dta.text('Discount');
                discountTotalElement.text((discountTotal / 1).toFixed(2));
                grandTotal= ((grandTotal / 1000) + parseFloat(shippingTotal));
                grandTotalElement.text((grandTotal  - discountTotal / 1).toFixed(2));
            }
        }    

        // set the other bottom table values
        subTotalElement.text((subTotal / 1000).toFixed(2));           
        totalTaxElement.text((taxTotal / 1000).toFixed(2)); 
         
    }

    $('#discountValue, #discountType, #shippingCost').on('change', function() {
        calculateTotals();
    });
 	

	// Delete all product/service rows and recalculate totals.
	$('#delete-all-rows').click(function() {
		$('#newInvoiceTable tbody tr').slice(0).remove();
		calculateTotals();
	});

	// Delete a product/service row and recalculate totals.
	$("#newInvoiceTable").on("click", ".delete-row", function (event) {
        $(this).closest("tr").remove();
        calculateTotals();
    });

    function invoiceDetails() {
    	var data={};
    	$("#invoice-details").find('select, input').each(function () {
	    	
		    data[$(this).attr('id')] = $(this).val() || $(this).text();       
		        
	    });
	    	var template = Handlebars.compile($('#invoiceDetails').html() );
    		$('.preview-invoice-details').append( template(data) );

    }

    function invoiceProducts() {

        $('#newInvoiceTable tbody').find('tr').each(function(){
            
            // get the value of each input field in a row, on the "name" attribute.
            var row={};
            $(this).find('input, select').each(function(){

                row[$(this).attr('name')]=$(this).val() || $(this).text();

            });
            
            // use handlebars.js to display the product rows in the preview modal.
            var template = Handlebars.compile($('#template').html() );
            $('.form-values').append( template(row) );

        });

        var data={};
        $('#newInvoiceTotalsTable tr').find('[id]').each(function() {           
                
                data[$(this).attr('id')] = $(this).val() || $(this).text();        
         
        }); 
        var totals = Handlebars.compile($('#previeuwTotals').html() );
        $('.totals').append( totals(data) );
         
    }

	// show preview of the invoice in a modal
    $("#invoicePreview").on('click', function(){
        var discount = $('#discountValue').val(),
            shipping = $('#shippingCost').val();

        // first delete all values
        $('.form-values').find('tr').empty();
        $('.totals').find('tr').remove();
        $('.preview-invoice-details').find('div').remove();

        setTimeout(function(){
            $('.discountTotalDisplay').hide();
            $('.shippingCostDisplay').hide();
        if (discount.length) {
            $('.discountTotalDisplay').show();
        }
        if (shipping.length) {
            $('.shippingCostDisplay').show();
        }

    }, 100);

		invoiceDetails();
        invoiceProducts();		        
	    	
	});  

	$( "#invoiceDate" ).pickadate();

  	$('#ivd').on('click', function() {
        $('#invoiceDate').pickadate('show');
    });
    
    $( "#invoiceExperationDate" ).pickadate({
        selectMonths: true, // Creates a dropdown to control month
        selectYears: 5 // Creates a dropdown of 5 years to control year
    });

	$('#ived').on('click', function() {
        $('#invoiceExperationDate').pickadate('show');
    });

    $('#newInvoiceTable').responsiveTable();

});