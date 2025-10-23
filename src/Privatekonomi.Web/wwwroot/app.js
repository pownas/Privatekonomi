// Testing file...

// // Download file function for CSV export
// window.downloadFile = function(filename, contentType, base64Content) {
//     const blob = base64ToBlob(base64Content, contentType);
//     const url = URL.createObjectURL(blob);
//     const link = document.createElement('a');
//     link.href = url;
//     link.download = filename;
//     document.body.appendChild(link);
//     link.click();
//     document.body.removeChild(link);
//     URL.revokeObjectURL(url);
// };

// function base64ToBlob(base64, contentType) {
//     const byteCharacters = atob(base64);
//     const byteNumbers = new Array(byteCharacters.length);
//     for (let i = 0; i < byteCharacters.length; i++) {
//         byteNumbers[i] = byteCharacters.charCodeAt(i);
//     }
//     const byteArray = new Uint8Array(byteNumbers);
//     return new Blob([byteArray], { type: contentType });
// }

// // Theme management functions
// window.themeManager = {
//     getTheme: function() {
//         return localStorage.getItem('darkMode') === 'true';
//     },
//     setTheme: function(isDarkMode) {
//         localStorage.setItem('darkMode', isDarkMode.toString());
//     },
//     hasPreference: function() {
//         return localStorage.getItem('darkMode') !== null;
//     }
// };

// // Debug functions for button click issues
// window.buttonDebugger = {
//     init: function() {
//         console.log('🔧 Button debugger initialized');
        
//         // Monitor all click events
//         document.addEventListener('click', function(event) {
//             const target = event.target;
//             const isMudButton = target.closest('.mud-button, .mud-icon-button, button');
            
//             if (isMudButton) {
//                 console.log('🔘 Button click detected:', {
//                     target: target,
//                     closest: isMudButton,
//                     classes: isMudButton.className,
//                     disabled: isMudButton.disabled,
//                     event: event
//                 });
                
//                 // Check if the button is disabled
//                 if (isMudButton.disabled) {
//                     console.warn('⚠️ Clicked button is disabled!');
//                 }
                
//                 // Check for preventDefault
//                 const originalPreventDefault = event.preventDefault;
//                 event.preventDefault = function() {
//                     console.warn('⚠️ preventDefault called on button click!');
//                     return originalPreventDefault.call(this);
//                 };
//             }
//         }, true); // Use capture phase
        
//         // Monitor form submissions
//         document.addEventListener('submit', function(event) {
//             console.log('📝 Form submission detected:', event);
//         });
        
//         // Monitor Blazor-specific events
//         window.addEventListener('blazor:beforeServerSideRender', function() {
//             console.log('🔄 Blazor: beforeServerSideRender');
//         });
        
//         window.addEventListener('blazor:afterServerSideRender', function() {
//             console.log('✅ Blazor: afterServerSideRender');
//         });
        
//         // Monitor Blazor connection state
//         if (window.Blazor) {
//             console.log('🔗 Blazor object found, setting up connection monitoring');
            
//             // Try to hook into Blazor reconnection events
//             const originalReconnect = window.Blazor.reconnect;
//             if (originalReconnect) {
//                 window.Blazor.reconnect = function() {
//                     console.log('🔄 Blazor reconnection attempt...');
//                     return originalReconnect.apply(this, arguments);
//                 };
//             }
//         } else {
//             console.log('⚠️ Blazor object not found yet, will monitor later');
//             // Monitor for when Blazor becomes available
//             let blazorCheckInterval = setInterval(() => {
//                 if (window.Blazor) {
//                     console.log('🔗 Blazor object now available');
//                     clearInterval(blazorCheckInterval);
//                 }
//             }, 100);
//         }
//     },
    
//     testConsole: function() {
//         console.log('🧪 Console test - if you see this, JavaScript is working!');
//         return true;
//     }
// };

// // Initialize button debugger when DOM is ready
// if (document.readyState === 'loading') {
//     document.addEventListener('DOMContentLoaded', window.buttonDebugger.init);
// } else {
//     window.buttonDebugger.init();
// }
