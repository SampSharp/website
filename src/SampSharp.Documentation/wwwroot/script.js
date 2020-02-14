
hljs.initHighlightingOnLoad();

// version dropdown
document.querySelectorAll('.dropdown')
    .forEach(function (dropdown) {
        dropdown.addEventListener('click', function (ev) {
            ev.stopPropagation();// Don't let clicks inside close via body
        });
        document.querySelector('body').addEventListener('click', function () {
            dropdown.classList.remove('open');// Close on click outside
        });
        dropdown.querySelector('a').addEventListener('click', function (ev) {
            dropdown.classList.toggle('open');// Toggle open on click button
            ev.preventDefault();
        });
    });

// expanding clicking
document.querySelectorAll('.tree-expander')
    .forEach(function(expander) {
        expander.addEventListener('click',
            function(ev) {
                expander.parentElement.classList.toggle('is-expanded');
            });
    });

// auto expand active page
window.onload = function() {
    document.querySelectorAll('.tree-group a')
        .forEach(function(anchor) {
            if (anchor.href === location.protocol + '//' + location.host + location.pathname) {
                anchor.parentElement.classList.add('is-active');
                anchor.closest('.tree-item').classList.add('is-expanded');
            }
        });
};