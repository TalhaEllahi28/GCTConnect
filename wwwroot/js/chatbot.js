class ChatBot {
    constructor() {
        this.chatbotIcon = document.getElementById('chatbotIcon');
        this.chatbotContainer = document.getElementById('chatbotContainer');
        this.closeBtn = document.getElementById('closeBtn');
        this.messageInput = document.getElementById('messageInput');
        this.sendBtn = document.getElementById('sendBtn');
        this.chatMessages = document.getElementById('chatMessages');
        this.typingIndicator = document.getElementById('typingIndicator');

        this.isOpen = false;
        this.init();
    }

    init() {
        // Event listeners
        this.chatbotIcon.addEventListener('click', () => this.openChatbot());
        this.closeBtn.addEventListener('click', () => this.closeChatbot());
        this.sendBtn.addEventListener('click', () => this.sendMessage());
        this.messageInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                this.sendMessage();
            }
        });

        // Auto-resize input
        this.messageInput.addEventListener('input', () => {
            this.updateSendButton();
        });

        this.updateSendButton();
    }

    openChatbot() {
        if (this.isOpen) return;

        this.isOpen = true;
        this.chatbotIcon.style.display = 'none';
        this.chatbotContainer.classList.add('active');

        // Focus on input after animation
        setTimeout(() => {
            this.messageInput.focus();
        }, 400);
    }

    closeChatbot() {
        if (!this.isOpen) return;

        this.isOpen = false;
        this.chatbotContainer.classList.remove('active');

        // Show icon after animation
        setTimeout(() => {
            this.chatbotIcon.style.display = 'flex';
        }, 400);
    }

    updateSendButton() {
        const hasText = this.messageInput.value.trim().length > 0;
        this.sendBtn.disabled = !hasText;
    }

    async sendMessage() {
        const message = this.messageInput.value.trim();
        if (!message) return;

        // Add user message
        this.addMessage(message, 'user');
        this.messageInput.value = '';
        this.updateSendButton();

        // Show typing indicator
        this.showTypingIndicator();

        try {
            // Call your API endpoint
            const response = await fetch('/api/chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ QueryText: message })
            });

            if (!response.ok) {
                throw new Error('API request failed');
            }

            const data = await response.json();
            this.hideTypingIndicator();
            this.addMessage(data.response, 'bot');
        } catch (error) {
            console.error('Error:', error);
            this.hideTypingIndicator();
            this.addMessage("I'm having trouble connecting right now. Please try again later.", 'bot');
        }
    }

    addMessage(content, sender) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${sender}-message`;

        const contentDiv = document.createElement('div');
        contentDiv.className = 'message-content-chatbot';
        contentDiv.innerHTML = `<p>${this.escapeHtml(content)}</p>`;

        const timeDiv = document.createElement('div');
        timeDiv.className = 'message-time';
        timeDiv.textContent = this.getCurrentTime();

        messageDiv.appendChild(contentDiv);
        messageDiv.appendChild(timeDiv);
        this.chatMessages.appendChild(messageDiv);

        // Scroll to bottom
        this.scrollToBottom();
    }

    showTypingIndicator() {
        this.typingIndicator.classList.add('active');
        this.scrollToBottom();
    }

    hideTypingIndicator() {
        this.typingIndicator.classList.remove('active');
    }

    scrollToBottom() {
        setTimeout(() => {
            this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
        }, 100);
    }

    getCurrentTime() {
        const now = new Date();
        const hours = now.getHours().toString().padStart(2, '0');
        const minutes = now.getMinutes().toString().padStart(2, '0');
        return `${hours}:${minutes}`;
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// Initialize chatbot when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ChatBot();
});

// Handle window resize
window.addEventListener('resize', () => {
    // Ensure proper scaling on mobile
    const chatbotContainer = document.getElementById('chatbotContainer');
    if (window.innerWidth <= 768 && chatbotContainer.classList.contains('active')) {
        chatbotContainer.style.height = `${window.innerHeight - 20}px`;
    }
});

// Add keyboard navigation support
document.addEventListener('keydown', (e) => {
    const chatbotContainer = document.getElementById('chatbotContainer');

    // Close chatbot with Escape key
    if (e.key === 'Escape' && chatbotContainer.classList.contains('active')) {
        document.getElementById('closeBtn').click();
    }
});