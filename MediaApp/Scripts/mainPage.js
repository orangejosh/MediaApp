$(document).ready(function () {
    setDotSpacing();
    setCurveHeight();
})

$(window).on('resize', function () {
    setDotSpacing();
    setCurveHeight();
})

window.onload = function () {
    animateMovies();
}

function setDotSpacing() {
    var winWidth = $(window).width();
    var marginLeft = (winWidth - 300) / 4;
    $('.dotContainer').css('margin-left', marginLeft);
}

function setCurveHeight() {
    var winHeight = $(window).height();
    var height = winHeight - 150;
    $('#curve').css('height', height);
}

function animateMovies() {
    $('.movieSlide').animate({ right: '0px' }, {easing: "linear", duration: 100000 });
}
