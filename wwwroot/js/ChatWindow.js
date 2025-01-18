connection.on('ReceiveMessage', function (user, message) {
    const chatContainer = document.getElementById('messages-container');

    const currentUser = document.getElementById("messages-container").dataset.currentUser;
    document.querySelectorAll(".message").forEach((msg) => {
        const sender = msg.dataset.sender;
        if (sender === currentUser) {
            msg.classList.add("my-message");
        } else {
            msg.classList.add("other-message");
        }
    });
    const messageAlignment = (user === currentUser) ? 'my-message' : 'other-message';

    const newMessage = `
        <div class="message ${messageAlignment}">
            <div class="message-card">
                <div class="message-header">
                    <strong class="message-sender">${user}</strong>
                </div>
                <div class="message-content">
                    ${message}
                </div>
                <div class="message-timestamp">
                    ${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </div>
            </div>
        </div>`;
    chatContainer.innerHTML += newMessage;

    // Scroll to the bottom of the chat container
    chatContainer.scrollTop = chatContainer.scrollHeight;
});
