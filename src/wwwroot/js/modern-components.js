/* ===================================================================
   Azure Naming Tool - Modern Component JavaScript
   Version: 1.0
   Date: October 17, 2025
   Description: JavaScript handlers for modern interactive components
                Replaces Bootstrap's JavaScript functionality
   =================================================================== */

// Initialize all components when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    initializeCollapsibles();
    initializeModals();
    initializeDropdowns();
    initializeDismissibles();
});

/* ===================================================================
   COLLAPSIBLE/ACCORDION COMPONENTS
   Replaces Bootstrap's data-bs-toggle="collapse" functionality
   =================================================================== */

function initializeCollapsibles() {
    // Handle all elements with data-bs-toggle="collapse"
    document.querySelectorAll('[data-bs-toggle="collapse"]').forEach(function(trigger) {
        // Set initial state
        const targetSelector = trigger.getAttribute('href') || trigger.getAttribute('data-bs-target');
        if (!targetSelector) return;
        
        const target = document.querySelector(targetSelector);
        if (!target) return;
        
        // Initialize aria-expanded based on current state
        const isExpanded = target.classList.contains('show');
        trigger.setAttribute('aria-expanded', isExpanded);
        
        // Add click handler
        trigger.addEventListener('click', function(e) {
            e.preventDefault();
            toggleCollapse(trigger, target);
        });
    });
}

function toggleCollapse(trigger, target) {
    const isCurrentlyExpanded = trigger.getAttribute('aria-expanded') === 'true';
    
    if (isCurrentlyExpanded) {
        // Collapse
        target.classList.remove('show');
        trigger.setAttribute('aria-expanded', 'false');
        trigger.classList.remove('expanded');
    } else {
        // Expand
        target.classList.add('show');
        trigger.setAttribute('aria-expanded', 'true');
        trigger.classList.add('expanded');
    }
}

/* ===================================================================
   MODAL COMPONENTS
   Replaces Bootstrap's modal functionality
   =================================================================== */

window.ModernModal = {
    /**
     * Show a modal by ID
     * @param {string} modalId - The ID of the modal element
     */
    show: function(modalId) {
        const modal = document.getElementById(modalId);
        if (!modal) {
            console.error(`Modal with ID "${modalId}" not found`);
            return;
        }
        
        // Create backdrop
        const backdrop = document.createElement('div');
        backdrop.className = 'modern-modal-backdrop';
        backdrop.id = modalId + '-backdrop';
        backdrop.addEventListener('click', function() {
            ModernModal.hide(modalId);
        });
        
        document.body.appendChild(backdrop);
        
        // Show modal with animation
        setTimeout(function() {
            backdrop.classList.add('show');
            modal.classList.add('show');
        }, 10);
        
        // Prevent body scroll
        document.body.style.overflow = 'hidden';
        
        // Focus trap
        const focusableElements = modal.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
        if (focusableElements.length > 0) {
            focusableElements[0].focus();
        }
        
        // ESC key handler
        const escHandler = function(e) {
            if (e.key === 'Escape') {
                ModernModal.hide(modalId);
                document.removeEventListener('keydown', escHandler);
            }
        };
        document.addEventListener('keydown', escHandler);
        modal.dataset.escHandler = 'attached';
    },
    
    /**
     * Hide a modal by ID
     * @param {string} modalId - The ID of the modal element
     */
    hide: function(modalId) {
        const modal = document.getElementById(modalId);
        const backdrop = document.getElementById(modalId + '-backdrop');
        
        if (modal) {
            modal.classList.remove('show');
        }
        
        if (backdrop) {
            backdrop.classList.remove('show');
            setTimeout(function() {
                backdrop.remove();
            }, 150); // Match CSS animation duration
        }
        
        // Restore body scroll
        document.body.style.overflow = '';
    },
    
    /**
     * Toggle a modal by ID
     * @param {string} modalId - The ID of the modal element
     */
    toggle: function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal && modal.classList.contains('show')) {
            this.hide(modalId);
        } else {
            this.show(modalId);
        }
    }
};

function initializeModals() {
    // Handle all elements with data-bs-toggle="modal"
    document.querySelectorAll('[data-bs-toggle="modal"]').forEach(function(trigger) {
        const targetSelector = trigger.getAttribute('data-bs-target') || trigger.getAttribute('href');
        if (!targetSelector) return;
        
        const modalId = targetSelector.replace('#', '');
        
        trigger.addEventListener('click', function(e) {
            e.preventDefault();
            ModernModal.show(modalId);
        });
    });
    
    // Handle all elements with data-bs-dismiss="modal"
    document.querySelectorAll('[data-bs-dismiss="modal"]').forEach(function(trigger) {
        trigger.addEventListener('click', function(e) {
            e.preventDefault();
            const modal = trigger.closest('.modern-modal');
            if (modal) {
                ModernModal.hide(modal.id);
            }
        });
    });
}

/* ===================================================================
   DROPDOWN COMPONENTS
   Replaces Bootstrap's dropdown functionality
   =================================================================== */

window.ModernDropdown = {
    /**
     * Toggle a dropdown by ID
     * @param {string} dropdownId - The ID of the dropdown menu element
     */
    toggle: function(dropdownId) {
        const dropdown = document.getElementById(dropdownId);
        if (!dropdown) return;
        
        const isVisible = dropdown.classList.contains('show');
        
        // Close all other dropdowns
        document.querySelectorAll('.modern-dropdown-menu.show').forEach(function(menu) {
            if (menu.id !== dropdownId) {
                menu.classList.remove('show');
            }
        });
        
        // Toggle this dropdown
        if (isVisible) {
            dropdown.classList.remove('show');
        } else {
            dropdown.classList.add('show');
        }
    },
    
    /**
     * Close all dropdowns
     */
    closeAll: function() {
        document.querySelectorAll('.modern-dropdown-menu.show').forEach(function(menu) {
            menu.classList.remove('show');
        });
    }
};

function initializeDropdowns() {
    // Handle all elements with data-bs-toggle="dropdown"
    document.querySelectorAll('[data-bs-toggle="dropdown"]').forEach(function(trigger) {
        const targetSelector = trigger.getAttribute('data-bs-target') || trigger.getAttribute('href');
        if (!targetSelector) return;
        
        const dropdownId = targetSelector.replace('#', '');
        
        trigger.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            ModernDropdown.toggle(dropdownId);
        });
    });
    
    // Close dropdowns when clicking outside
    document.addEventListener('click', function(e) {
        if (!e.target.closest('.modern-dropdown')) {
            ModernDropdown.closeAll();
        }
    });
}

/* ===================================================================
   DISMISSIBLE COMPONENTS
   Replaces Bootstrap's alert dismissible functionality
   =================================================================== */

function initializeDismissibles() {
    // Handle all elements with data-bs-dismiss attribute
    document.querySelectorAll('[data-bs-dismiss]').forEach(function(trigger) {
        const dismissType = trigger.getAttribute('data-bs-dismiss');
        
        if (dismissType === 'alert' || dismissType === 'notification') {
            trigger.addEventListener('click', function(e) {
                e.preventDefault();
                const parent = trigger.closest('.modern-notification, .modern-alert, .alert');
                if (parent) {
                    parent.style.opacity = '0';
                    parent.style.transform = 'translateX(20px)';
                    parent.style.transition = 'all 0.2s ease';
                    
                    setTimeout(function() {
                        parent.remove();
                    }, 200);
                }
            });
        }
    });
}

/* ===================================================================
   TOOLTIP/POPOVER HELPERS
   Basic tooltip functionality (can be enhanced later)
   =================================================================== */

window.ModernTooltip = {
    /**
     * Show a simple tooltip
     * @param {HTMLElement} element - The element to show tooltip for
     * @param {string} text - The tooltip text
     */
    show: function(element, text) {
        const tooltip = document.createElement('div');
        tooltip.className = 'modern-tooltip';
        tooltip.textContent = text;
        tooltip.style.cssText = `
            position: absolute;
            background: rgba(0, 0, 0, 0.9);
            color: white;
            padding: 6px 12px;
            border-radius: 4px;
            font-size: 12px;
            z-index: 9999;
            pointer-events: none;
            white-space: nowrap;
        `;
        
        document.body.appendChild(tooltip);
        
        const rect = element.getBoundingClientRect();
        tooltip.style.top = (rect.top - tooltip.offsetHeight - 5) + 'px';
        tooltip.style.left = (rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2)) + 'px';
        
        return tooltip;
    },
    
    /**
     * Hide a tooltip
     * @param {HTMLElement} tooltip - The tooltip element to hide
     */
    hide: function(tooltip) {
        if (tooltip && tooltip.parentNode) {
            tooltip.remove();
        }
    }
};

/* ===================================================================
   UTILITY FUNCTIONS
   =================================================================== */

/**
 * Smooth scroll to an element
 * @param {string} elementId - The ID of the element to scroll to
 */
window.smoothScrollTo = function(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({
            behavior: 'smooth',
            block: 'start'
        });
    }
};

/**
 * Copy text to clipboard
 * @param {string} text - The text to copy
 * @returns {Promise<boolean>} - Success status
 */
window.copyToClipboard = async function(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Failed to copy text:', err);
        return false;
    }
};

/**
 * Show a toast notification
 * @param {string} message - The message to display
 * @param {string} type - The type (info, success, warning, danger)
 * @param {number} duration - Duration in milliseconds (default 3000)
 */
window.showToast = function(message, type = 'info', duration = 3000) {
    const toast = document.createElement('div');
    toast.className = `modern-notification modern-notification-${type}`;
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        animation: slideInRight 0.3s ease;
    `;
    toast.textContent = message;
    
    document.body.appendChild(toast);
    
    setTimeout(function() {
        toast.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(function() {
            toast.remove();
        }, 300);
    }, duration);
};

// Add CSS animations for toast
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

console.log('Modern Components JavaScript initialized successfully');
