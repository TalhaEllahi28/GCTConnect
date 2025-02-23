document.addEventListener("DOMContentLoaded", function () {
    fetch('/GetGroupsAndMessages/GetUserGroups', {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    })
        .then(response => response.json())
        .then(groups => {
            let groupList = '';
            groups.forEach(group => {
                groupList += `
                <li>
                    <button id="group-button" class="group-item" onclick="selectGroup(this); loadMessages(${group.groupId}, '${group.groupName}')">
                        ${group.groupName}
                    </button>
                </li>`;
            });
            document.querySelector('#groups-ul').innerHTML = groupList;
            const chatContainer = document.getElementById('messages-container');
            chatContainer.scrollTop = chatContainer.scrollHeight;

        })
        .catch(error => console.error('Error fetching groups:', error));
});

// Function to set the clicked group as active
function selectGroup(elem) {
    var buttons = document.querySelectorAll(".group-item");
    buttons.forEach(function (btn) {
        btn.classList.remove("active");
    });
    elem.classList.add("active");
}

function loadMessages(groupId, groupName) {
    fetch(`/GetGroupsAndMessages/GetGroupMessages?groupId=${groupId}`, {
        method: 'GET',
        headers: { 'Content-Type': 'application/json' }
    })
        .then(response => response.text())
        .then(partialViewHtml => {
            document.querySelector('#chat-container').innerHTML = partialViewHtml;
            setupSignalR(groupName); // Setup SignalR for the selected group
            attachSendButtonListener(groupName);
        })
        .catch(error => console.error('Error loading messages:', error));
}
let connection;

// SignalR Setup
function setupSignalR(groupName) {
    if (!connection) {
        connection = new signalR.HubConnectionBuilder()
            .withUrl('/Services/ChatHub') // Ensure correct endpoint
            .build();

        connection.onreconnecting((error) => {
            console.log('Connection lost, attempting to reconnect...', error);
        });

        connection.onreconnected(() => {
            console.log('Connection reestablished.');
            joinGroup(groupName); // Ensure the group is re-joined after reconnection
        });

        connection.onclose(async (error) => {
            console.log('Connection closed. Attempting to restart...', error);
            try {
                await connection.start();
                console.log('Reconnected successfully.');
                joinGroup(groupName);
            } catch (err) {
                console.error('Failed to reconnect:', err);
            }
        });


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





        //connection.on('ReceiveMessage', function (user, message) {
        //    const chatContainer = document.getElementById('messages-container');
        //    chatContainer.scrollTop = chatContainer.scrollHeight;
        //    if (chatContainer) {
        //        const currentUser = chatContainer.dataset.currentUser;
        //        const messageClass = user === currentUser ? "my-message" : "other-message";
        //        const messageElement = document.createElement('div');
        //        messageElement.classList.add('message', messageClass);
        //        messageElement.setAttribute('data-sender', user);
        //        const messageCard = document.createElement('div');
        //        messageCard.classList.add('message-card');
        //        const messageHeader = document.createElement('div');
        //        messageHeader.classList.add('message-header');
        //        messageHeader.textContent = user;
        //        const messageContent = document.createElement('div');
        //        messageContent.textContent = message;
        //        const messageTimestamp = document.createElement('div');
        //        messageTimestamp.classList.add('message-timestamp');
        //        messageTimestamp.textContent = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        //        messageCard.appendChild(messageHeader);
        //        messageCard.appendChild(messageContent);
        //        messageCard.appendChild(messageTimestamp);
        //        messageElement.appendChild(messageCard);
        //        chatContainer.appendChild(messageElement);
        //        chatContainer.scrollTop = chatContainer.scrollHeight;
        //    }
        //});

        connection.start()
            .then(() => {
                console.log('SignalR connection started.');
                joinGroup(groupName);
            })
            .catch(err => console.error('Error starting SignalR connection:', err));
    } else {
        joinGroup(groupName);
    }
}

function joinGroup(groupName) {
    if (connection) {
        console.log("GroupName: ", groupName);
        connection.invoke('JoinGroup', groupName)
            .then(() => console.log(`Joined group ${groupName}`))
            .catch(err => console.error('Error joining SignalR group:', err));
    }
}

function sendMessage(groupName) {
    const messageInput = document.getElementById('message-input');
    const message = messageInput ? messageInput.value.trim() : '';

    if (message === '') {
        alert('Message cannot be empty.');
        return;
    }
    connection.invoke('SendMessage', groupName, message)
        .then(() => {
            if (messageInput) {
                messageInput.value = ''; // Clear input after sending
            }
        })
        .catch(err => console.error('Error sending message:', err));
}

function attachSendButtonListener(groupName) {
    const sendButton = document.getElementById('send-button');
    sendButton.onclick = function () {
        sendMessage(groupName);
    };
}
