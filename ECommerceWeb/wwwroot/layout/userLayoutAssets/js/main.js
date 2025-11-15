(function ($) {
    "use strict";

    /*[ Load page ]
    ===========================================================*/
    $(".animsition").animsition({
        inClass: 'fade-in',
        outClass: 'fade-out',
        inDuration: 1500,
        outDuration: 800,
        linkElement: '.animsition-link',
        loading: true,
        loadingParentElement: 'html',
        loadingClass: 'animsition-loading-1',
        loadingInner: '<div class="loader05"></div>',
        timeout: false,
        timeoutCountdown: 5000,
        onLoadEvent: true,
        browser: ['animation-duration', '-webkit-animation-duration'],
        overlay: false,
        overlayClass: 'animsition-overlay-slide',
        overlayParentElement: 'html',
        transition: function (url) { window.location.href = url; }
    });

    /*[ Back to top ]
    ===========================================================*/
    var windowH = $(window).height() / 2;

    $(window).on('scroll', function () {
        if ($(this).scrollTop() > windowH) {
            $("#myBtn").css('display', 'flex');
        } else {
            $("#myBtn").css('display', 'none');
        }
    });

    $('#myBtn').on("click", function () {
        $('html, body').animate({ scrollTop: 0 }, 300);
    });


    /*==================================================================
    [ Fixed Header ]*/
    var headerDesktop = $('.container-menu-desktop');
    var wrapMenu = $('.wrap-menu-desktop');

    if ($('.top-bar').length > 0) {
        var posWrapHeader = $('.top-bar').height();
    }
    else {
        var posWrapHeader = 0;
    }


    if ($(window).scrollTop() > posWrapHeader) {
        $(headerDesktop).addClass('fix-menu-desktop');
        $(wrapMenu).css('top', 0);
    }
    else {
        $(headerDesktop).removeClass('fix-menu-desktop');
        $(wrapMenu).css('top', posWrapHeader - $(this).scrollTop());
    }

    $(window).on('scroll', function () {
        if ($(this).scrollTop() > posWrapHeader) {
            $(headerDesktop).addClass('fix-menu-desktop');
            $(wrapMenu).css('top', 0);
        }
        else {
            $(headerDesktop).removeClass('fix-menu-desktop');
            $(wrapMenu).css('top', posWrapHeader - $(this).scrollTop());
        }
    });


    /*==================================================================
    [ Menu mobile ]*/
    $('.btn-show-menu-mobile').on('click', function () {
        $(this).toggleClass('is-active');
        $('.menu-mobile').slideToggle();
    });

    var arrowMainMenu = $('.arrow-main-menu-m');

    for (var i = 0; i < arrowMainMenu.length; i++) {
        $(arrowMainMenu[i]).on('click', function () {
            $(this).parent().find('.sub-menu-m').slideToggle();
            $(this).toggleClass('turn-arrow-main-menu-m');
        })
    }

    $(window).resize(function () {
        if ($(window).width() >= 992) {
            if ($('.menu-mobile').css('display') == 'block') {
                $('.menu-mobile').css('display', 'none');
                $('.btn-show-menu-mobile').toggleClass('is-active');
            }

            $('.sub-menu-m').each(function () {
                if ($(this).css('display') == 'block') {
                    console.log('hello');
                    $(this).css('display', 'none');
                    $(arrowMainMenu).removeClass('turn-arrow-main-menu-m');
                }
            });

        }
    });


    /*==================================================================
    [ Show / hide modal search ]*/
    $('.js-show-modal-search').on('click', function () {
        $('.modal-search-header').addClass('show-modal-search');
        $(this).css('opacity', '0');
    });

    $('.js-hide-modal-search').on('click', function () {
        $('.modal-search-header').removeClass('show-modal-search');
        $('.js-show-modal-search').css('opacity', '1');
    });

    $('.container-search-header').on('click', function (e) {
        e.stopPropagation();
    });


    /*==================================================================
    [ Isotope ]*/
    var $topeContainer = $('.isotope-grid');
    var $filter = $('.filter-tope-group');

    // filter items on button click
    $filter.each(function () {
        $filter.on('click', 'button', function () {
            var filterValue = $(this).attr('data-filter');
            $topeContainer.isotope({ filter: filterValue });
        });

    });

    // init Isotope
    $(window).on('load', function () {
        var $grid = $topeContainer.each(function () {
            $(this).isotope({
                itemSelector: '.isotope-item',
                layoutMode: 'fitRows',
                percentPosition: true,
                animationEngine: 'best-available',
                masonry: {
                    columnWidth: '.isotope-item'
                }
            });
        });
    });

    var isotopeButton = $('.filter-tope-group button');

    $(isotopeButton).each(function () {
        $(this).on('click', function () {
            for (var i = 0; i < isotopeButton.length; i++) {
                $(isotopeButton[i]).removeClass('how-active1');
            }

            $(this).addClass('how-active1');
        });
    });

    /*==================================================================
    [ Filter / Search product ]*/
    $('.js-show-filter').on('click', function () {
        $(this).toggleClass('show-filter');
        $('.panel-filter').slideToggle(400);

        if ($('.js-show-search').hasClass('show-search')) {
            $('.js-show-search').removeClass('show-search');
            $('.panel-search').slideUp(400);
        }
    });

    $('.js-show-search').on('click', function () {
        $(this).toggleClass('show-search');
        $('.panel-search').slideToggle(400);

        if ($('.js-show-filter').hasClass('show-filter')) {
            $('.js-show-filter').removeClass('show-filter');
            $('.panel-filter').slideUp(400);
        }
    });


    /*==================================================================
    [ Cart ]*/
    $('.js-show-cart').on('click', function () {
        $('.js-panel-cart').addClass('show-header-cart');
    });

    $('.js-hide-cart').on('click', function () {
        $('.js-panel-cart').removeClass('show-header-cart');
    });

    /*==================================================================
    [ Cart ]*/
    $('.js-show-sidebar').on('click', function () {
        $('.js-sidebar').addClass('show-sidebar');
    });

    $('.js-hide-sidebar').on('click', function () {
        $('.js-sidebar').removeClass('show-sidebar');
    });

    /*==================================================================
    [ +/- num product ]*/
    $('.btn-num-product-down').on('click', function () {
        var numProduct = Number($(this).next().val());
        if (numProduct > 0) $(this).next().val(numProduct - 1);
    });

    $('.btn-num-product-up').on('click', function () {
        var numProduct = Number($(this).prev().val());
        $(this).prev().val(numProduct + 1);
    });

    /*==================================================================
    [ Rating ]*/
    function setupRating() {
        $('.wrap-rating').each(function () {
            var item = $(this).find('.item-rating');
            var rated = -1;
            var input = $(this).find('input');
            $(input).val(0);

            $(item).on('mouseenter', function () {
                var index = item.index(this);
                var i = 0;
                for (i = 0; i <= index; i++) {
                    $(item[i]).removeClass('zmdi-star-outline');
                    $(item[i]).addClass('zmdi-star');
                }

                for (var j = i; j < item.length; j++) {
                    $(item[j]).addClass('zmdi-star-outline');
                    $(item[j]).removeClass('zmdi-star');
                }
            });

            $(item).on('click', function () {
                var index = item.index(this);
                rated = index;
                $(input).val(index + 1);
            });

            $(this).on('mouseleave', function () {
                var i = 0;
                for (i = 0; i <= rated; i++) {
                    $(item[i]).removeClass('zmdi-star-outline');
                    $(item[i]).addClass('zmdi-star');
                }

                for (var j = i; j < item.length; j++) {
                    $(item[j]).addClass('zmdi-star-outline');
                    $(item[j]).removeClass('zmdi-star');

                }
            });
        });
    }

    // Khởi tạo Rating khi trang tải lần đầu (cho trang Details.cshtml)
    setupRating();


  
    $('.js-show-modal1').off('click').on('click', function (e) {
        e.preventDefault();

        var quickViewUrl = $(this).attr('href'); // Lấy URL QuickView từ liên kết

        $('.js-modal1').addClass('show-modal1');
        // Hiển thị trạng thái tải trong modal
        $('.js-modal1 .wrap-modal1').html('<div class="text-center p-tb-60">Đang tải chi tiết sản phẩm...</div>');

        // Tải nội dung Quick View bằng AJAX
        $.get(quickViewUrl, function (data) {

            // 1. Xóa mọi khởi tạo slick cũ trước khi chèn nội dung mới (Để tránh lỗi nhiều lần khởi tạo)
            $('.js-modal1 .slick3').each(function () {
                if ($(this).hasClass('slick-initialized')) {
                    $(this).slick('unslick');
                }
            });
            $('.js-modal1 .wrap-slick3-dots').each(function () {
                if ($(this).hasClass('slick-initialized')) {
                    $(this).slick('unslick');
                }
            });

            // Chèn nội dung mới tải vào modal
            $('.js-modal1 .wrap-modal1').html(data);

            // 2. KHỞI TẠO SLICK SLIDER CHO NỘI DUNG MỚI TẢI
            var $modalSlick3 = $('.js-modal1 .slick3');
            var $modalSlick3Dots = $('.js-modal1 .wrap-slick3-dots');

            if ($modalSlick3.length > 0) {

                // Khởi tạo Slick Main Slider
                $modalSlick3.slick({
                    slidesToShow: 1,
                    slidesToScroll: 1,
                    arrows: true,
                    dots: true,
                    fade: true,
                    infinite: true,
                    asNavFor: $modalSlick3Dots,
                });

                // Khởi tạo Slick Dots/Nav Slider
                $modalSlick3Dots.slick({
                    slidesToShow: 4,
                    slidesToScroll: 1,
                    infinite: true,
                    focusOnSelect: true,
                    asNavFor: $modalSlick3,
                });

                // ********************************************************
                // FIX QUAN TRỌNG: Buộc Slick tính toán lại bố cục sau khi Modal hiển thị
                // ********************************************************
                setTimeout(function () {
                    $modalSlick3.slick('setPosition');
                    $modalSlick3Dots.slick('setPosition');
                }, 100);
            }

            // 3. Khởi tạo lại Rating và Magnific Popup cho nội dung Modal
            setupRating();

            $('.js-modal1 .gallery-lb').each(function () {
                $(this).magnificPopup({
                    delegate: 'a',
                    type: 'image',
                    gallery: { enabled: true },
                    mainClass: 'mfp-fade'
                });
            });

        }).fail(function (jqXHR, textStatus, errorThrown) {
            // Xử lý lỗi nếu AJAX thất bại
            console.error("Lỗi tải Quick View:", textStatus, errorThrown, jqXHR.status);
            $('.js-modal1 .wrap-modal1').html('<div class="text-center p-tb-60 text-danger">⚠️ LỖI: Không tải được thông tin sản phẩm. (Lỗi Code: ' + jqXHR.status + ')</div>');
        });
    });

    $('.js-hide-modal1').on('click', function () {
        $('.js-modal1').removeClass('show-modal1');
        // Kỹ thuật tốt: Hủy Slick khi Modal đóng để tránh xung đột
        var $modalSlick3 = $('.js-modal1 .slick3');
        if ($modalSlick3.hasClass('slick-initialized')) {
            $modalSlick3.slick('unslick');
            $('.js-modal1 .wrap-slick3-dots').slick('unslick');
        }
        // Xóa nội dung để chuẩn bị cho lần tải mới
        $('.js-modal1 .wrap-modal1').empty();
    });


})(jQuery);