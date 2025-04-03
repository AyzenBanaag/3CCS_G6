// Initialize tooltips
$(document).ready(function () {
    $('[data-toggle="tooltip"]').tooltip();

    // Smooth scrolling for anchor links
    $('a[href*="#"]').on('click', function (e) {
        e.preventDefault();
        $('html, body').animate(
            {
                scrollTop: $($(this).attr('href')).offset().top,
            },
            500,
            'linear'
        );
    });
});