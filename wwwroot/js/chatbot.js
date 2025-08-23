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

    sendMessage() {
        const message = this.messageInput.value.trim();
        if (!message) return;

        // Add user message
        this.addMessage(message, 'user');
        this.messageInput.value = '';
        this.updateSendButton();

        // Show typing indicator
        this.showTypingIndicator();

        // Simulate AI response
        setTimeout(() => {
            this.hideTypingIndicator();
            this.addBotResponse(message);
        }, Math.random() * 2000 + 1000); // 1-3 seconds delay
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

    addBotResponse(userMessage) {
        // Simple response logic - in a real app, this would be an API call
        const responses = [
            "That's an interesting question! I'm here to help you with any information you need.",
            "I understand what you're asking. Let me provide you with a helpful response.",
            "Great question! I'm processing your request and here's what I think...",
            "Thank you for your message. I'm designed to assist you with various tasks and questions.",
            "I appreciate you reaching out. Based on your input, here's my response...",
            "That's a thoughtful inquiry. Let me share some insights on that topic.",
            "I'm glad you asked! Here's what I can tell you about that...",
            "Excellent point! I'm here to provide you with accurate and helpful information."
        ];

        const randomResponse = responses[Math.floor(Math.random() * responses.length)];
        this.addMessage(randomResponse, 'bot');
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

// Add smooth scrolling behavior for better UX
document.addEventListener('scroll', (e) => {
    if (e.target.classList && e.target.classList.contains('chat-messages')) {
        e.preventDefault();
    }
});

// Prevent zoom on iOS when focusing input
document.addEventListener('touchstart', {});

// Add keyboard navigation support
document.addEventListener('keydown', (e) => {
    const chatbotContainer = document.getElementById('chatbotContainer');

    // Close chatbot with Escape key
    if (e.key === 'Escape' && chatbotContainer.classList.contains('active')) {
        document.getElementById('closeBtn').click();
    }
});