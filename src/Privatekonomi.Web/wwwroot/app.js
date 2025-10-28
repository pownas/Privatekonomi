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
        // Also apply the theme class immediately
        if (isDarkMode) {
            document.documentElement.classList.add('mud-theme-dark');
        } else {
            document.documentElement.classList.remove('mud-theme-dark');
        }
    },
    hasPreference: function() {
        return localStorage.getItem('darkMode') !== null;
    },
    // Get the initial theme state (what was pre-applied in the head script)
    // NOTE: This logic mirrors the inline script in App.razor <head>
    // Both must stay in sync to prevent theme flash on page load
    getInitialTheme: function() {
        var darkMode = localStorage.getItem('darkMode');
        if (darkMode === 'true') {
            return true;
        } else if (darkMode === 'false') {
            return false;
        } else {
            // No preference saved, check system preference
            return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
        }
    }
};

// Keyboard shortcuts manager
window.keyboardShortcuts = {
    init: function(dotNetHelper) {
        // Store reference to .NET helper
        this.dotNetHelper = dotNetHelper;
        
        // Add keyboard event listener
        document.addEventListener('keydown', (e) => {
            // Ignore if user is typing in an input field
            if (e.target.tagName === 'INPUT' || 
                e.target.tagName === 'TEXTAREA' || 
                e.target.isContentEditable) {
                return;
            }
            
            // Ctrl/Cmd + N: New Transaction
            if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
                e.preventDefault();
                window.location.href = '/transactions/new';
            }
            // Ctrl/Cmd + B: Budgets
            else if ((e.ctrlKey || e.metaKey) && e.key === 'b') {
                e.preventDefault();
                window.location.href = '/budgets';
            }
            // Ctrl/Cmd + T: Transactions
            else if ((e.ctrlKey || e.metaKey) && e.key === 't') {
                e.preventDefault();
                window.location.href = '/transactions';
            }
            // Ctrl/Cmd + H: Home/Dashboard
            else if ((e.ctrlKey || e.metaKey) && e.key === 'h') {
                e.preventDefault();
                window.location.href = '/';
            }
            // Ctrl/Cmd + G: Goals
            else if ((e.ctrlKey || e.metaKey) && e.key === 'g') {
                e.preventDefault();
                window.location.href = '/goals';
            }
            // Ctrl/Cmd + I: Investments
            else if ((e.ctrlKey || e.metaKey) && e.key === 'i') {
                e.preventDefault();
                window.location.href = '/investments';
            }
            // Ctrl/Cmd + K: Calendar
            else if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                window.location.href = '/transactions/calendar';
            }
            // Ctrl/Cmd + L: Tags
            else if ((e.ctrlKey || e.metaKey) && e.key === 'l') {
                e.preventDefault();
                window.location.href = '/tags';
            }
            // Ctrl/Cmd + /: Show keyboard shortcuts help
            else if ((e.ctrlKey || e.metaKey) && e.key === '/') {
                e.preventDefault();
                if (this.dotNetHelper) {
                    this.dotNetHelper.invokeMethodAsync('ShowKeyboardShortcutsHelp');
                }
            }
        });
    },
    
    dispose: function() {
        // Remove event listener if needed
        this.dotNetHelper = null;
    }
};

// PWA Service Worker registration
window.pwaManager = {
    register: function() {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/service-worker.js')
                .then((registration) => {
                    console.log('Service Worker registered:', registration);
                })
                .catch((error) => {
                    console.log('Service Worker registration failed:', error);
                });
        }
    },
    
    isInstallable: function() {
        return 'serviceWorker' in navigator && 'BeforeInstallPromptEvent' in window;
    }
};