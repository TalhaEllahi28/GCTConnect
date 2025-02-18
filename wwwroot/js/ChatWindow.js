connection.on('ReceiveMessage', function (user, message, profilePic) {
    const chatContainer = document.getElementById('messages-container');
    const currentUser = chatContainer.dataset.currentUser;
    const messageClass = (user === currentUser) ? "sent" : "received";

    const newMessage = `
        <div class="message-container ${messageClass}" data-sender="${user}">
            <div class="avatar-wrapper">
                <img src="${profilePic}" alt="${user}" class="avatar-image"/>
            </div>
            <div class="message-content">
                <div class="message-bubble">
                    <p>${message}</p>
                </div>
                <span class="timestamp">
                    ${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </span>
            </div>
        </div>
    `;
    chatContainer.innerHTML += newMessage;
    chatContainer.scrollTop = chatContainer.scrollHeight;
});
