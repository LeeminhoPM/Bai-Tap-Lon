$(document).ready(function () {
    $(window).scroll(function () {
        var st = $(this).scrollTop();
        if (st > 0) {
            $("#header-logo, #header-navbar, header").removeClass("home-top");
            $("#header-logo, #header-navbar, header").addClass("default");
        } else {
            $("#header-logo, #header-navbar, header").removeClass("default");
            $("#header-logo, #header-navbar, header").addClass("home-top");
        }

        userDropbarClose();
    });

    // Menu sidebar
    $("#header-menu").click(function () {
        $("#header-menu-sidebar").css("transition-delay", "0s");
        $("#header-menu-sidebar button, #header-menu-sidebar li").css(
            "transition-delay",
            "0.5s"
        );
        $("#overlay-all").css("transition-delay", "0s");

        $("#header-menu-sidebar, #overlay-all").removeClass("sidebar-close");
        $("#header-menu-sidebar, #overlay-all").addClass("sidebar-open");
        $("body").css("overflowY", "hidden");
    });
    $("#sidebar-top-container button").click(function () {
        $("body").css("overflowY", "visible");
        $("#header-menu-sidebar li, #header-menu-sidebar button").css(
            "transition-delay",
            "0s"
        );
        $("#overlay-all").css("transition-delay", "0.3s");

        $("#header-menu-sidebar, #overlay-all").removeClass("sidebar-open");
        $("#header-menu-sidebar, #overlay-all").addClass("sidebar-close");
    });

    // User dropbar
    var isOpen = false;

    function userDropbarOpen() {
        $("#user-identity, #user-identity.dropbar-open > ul li").addClass(
            "dropbar-open"
        );
        $("#user-identity, #user-identity.dropbar-open > ul li").removeClass(
            "dropbar-close"
        );
        $("#user-identity").css("transition-delay", "0s");
        $("#user-identity > ul li").css("transition-delay", "0.3s");
        isOpen = true;
    }
    function userDropbarClose() {
        $("#user-identity, #user-identity.dropbar-open > ul li").removeClass(
            "dropbar-open"
        );
        $("#user-identity, #user-identity.dropbar-open > ul li").addClass(
            "dropbar-close"
        );
        $("#user-identity").css("transition-delay", "0.3s");
        $("#user-identity > ul li").css("transition-delay", "0s");
        isOpen = false;
    }

    $("#user-toggle-btn").click(function () {
        if (!isOpen) {
            userDropbarOpen();
        } else {
            userDropbarClose();
        }
    });
});
