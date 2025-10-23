// Download file function for CSV exports
window.downloadFile = function(filename, contentType, base64Content) {
    const blob = base64ToBlob(base64Content, contentType);
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// Helper function to convert base64 to Blob
function base64ToBlob(base64, contentType) {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: contentType });
}

// Theme management functions
window.themeManager = {
    getTheme: function() {
        return localStorage.getItem('darkMode') === 'true';
    },
    setTheme: function(isDarkMode) {
        localStorage.setItem('darkMode', isDarkMode.toString());
    },
    hasPreference: function() {
        return localStorage.getItem('darkMode') !== null;
    }
};