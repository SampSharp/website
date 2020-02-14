window.onload = function() {
    // expanding clicking tree thingies
    document.querySelectorAll('.tree-expander')
        .forEach(function(expander) {
            expander.addEventListener('click',
                function(ev) {
                    expander.parentElement.classList.toggle('is-expanded');
                });
        });

    // expanding menu thingiers
    document.querySelectorAll('[data-toggle-target]').forEach(function(source) {
        source.addEventListener('click',
            function(ev) {
                ev.preventDefault();
                ev.stopPropagation();

                var selector = source.dataset.toggleTarget;
                var claz = source.dataset.toggleClass;

                if (selector && claz) {
                    document.querySelectorAll(selector).forEach(function(target) {
                        target.classList.toggle(claz);
                    });
                }

                return false;
            });
    });

    // floating side scrolling thingies
    var floaters = document.querySelectorAll(".can-float");
    var head = document.querySelector(".head");
    
    floaters.forEach(function(f) {
        f.classList.add("is-floating"); // make element position fixed
    });

    function updateNav() {
        floaters.forEach(function(f) {
            // Style was overriden
            var disabled = getComputedStyle(f).getPropertyValue("--disable-float");
            if (disabled && parseInt(disabled)) {
                return;
            }

            // Stay under the header
            var ny = Math.max(head.clientHeight - window.scrollY, 0);

            // Stay within bounds of parent
            var parentRect = f.parentElement.getBoundingClientRect();
            var height = parentRect.bottom;
            var vpHeight = document.documentElement.clientHeight;
            
            // Stay within viewport
            height = Math.min(height, vpHeight) - ny;

            // Apply
            f.style.top = ny + 'px';
            f.style.height = height + 'px';
        });
    }

    window.addEventListener("resize", updateNav);
    window.onscroll = updateNav;

    updateNav();

    // close nav on clicking blur thingy
    document.querySelector('.nav-container')
        .addEventListener("click", function(e) {
            if (e.target !== this) {
                return;
            }
            var f = document.querySelector('.nav-container > .is-floating');
            var disabled = getComputedStyle(f).getPropertyValue("--disable-float");

            if (disabled && parseInt(disabled)) {
                document.querySelector('.nav-container').classList.remove('is-open');
            }
        });
};