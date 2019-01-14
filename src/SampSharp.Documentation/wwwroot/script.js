
hljs.initHighlightingOnLoad();

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