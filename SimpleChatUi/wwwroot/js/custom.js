window.scrollToElement = function (elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    } else {
        console.error(`scrollToElement: Element with ID '${elementId}' not found.`);
    }
}
