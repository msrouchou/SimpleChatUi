window.scrollToElement = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
        element.focus();
    } else {
        console.error(`scrollToElement: Element with ID '${elementId}' not found.`);
    }
}
