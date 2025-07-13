
// ========== ON DOM LOAD: Load Groups and Friends ==========
document.addEventListener("DOMContentLoaded", function () {
    loadUserGroups();
    loadFriends();
});

function loadUserGroups() {
    fetch('/GetGroupsAndMessages/GetUserGroups')
        .then(response => response.json())
        .then(groups => {
            let groupList = '';
            groups.forEach(group => {
                groupList += `
                    <li>
                        <div class="conversation-item" onclick="selectGroup(this); loadMessages(${group.groupId}, '${group.groupName}')">
                            <div class="conversation-avatar">
                                <div class="avatar">
                                    <div class="avatar-placeholder">${getInitials(group.groupName)}</div>
                                    <div class="status-indicator online"></div>
                                </div>
                            </div>
                            <div class="conversation-info">
                                <div class="conversation-header">
                                    <div class="conversation-name">${group.groupName}</div>
                                    <div class="conversation-time">Now</div>
                                </div>
                                <div class="conversation-last-message">
                                    Click to view messages
                                </div>
                            </div>
                        </div>
                    </li>
                `;
            });
            document.querySelector('#groups-ul').innerHTML = groupList;
        })
        .catch(error => console.error('Error fetching groups:', error));
}

function loadFriends() {
    fetch('/GetGroupsAndMessages/GetFriends')
        .then(response => response.json())
        .then(friends => {
            let friendList = '';
            friends.forEach(friend => {
                friendList += `
                    <li>
                        <div class="conversation-item" onclick="selectGroup(this); openChatWithFriend(${friend.userId}, '${friend.username}')">
                            <div class="conversation-avatar">
                                <div class="avatar">
                                    ${friend.profilePic ? `<img src="${friend.profilePic}" class="avatar-img" />` : `<div class="avatar-placeholder">${getInitials(friend.username)}</div>`}
                                    <div class="status-indicator online"></div>
                                </div>
                            </div>
                            <div class="conversation-info">
                                <div class="conversation-header">
                                    <div class="conversation-name">${friend.name} ${friend.lastName}</div>
                                    <div class="conversation-time">Now</div>
                                </div>
                                <div class="conversation-last-message">
                                    Click to start chat
                                </div>
                            </div>
                        </div>
                    </li>
                `;
            });
            document.querySelector('#friends-ul').innerHTML = friendList;
        })
        .catch(error => console.error('Error fetching friends:', error));
}

function getInitials(name) {
    if (!name) return '?';

    const words = name.trim().split(' ');
    if (words.length === 1) {
        return name.substring(0, 2).toUpperCase();
    } else {
        return (words[0].charAt(0) + words[words.length - 1].charAt(0)).toUpperCase();
    }
}

function selectGroup(elem) {
    const items = document.querySelectorAll('.conversation-item');
    items.forEach(item => item.classList.remove('active'));

    elem.classList.add("active");

    setTimeout(() => {
        const container = document.getElementById("messages-container");
        if (container) {
            container.scrollTop = container.scrollHeight;
        }
    }, 150);
}

function loadMessages(groupId, groupName) {
    fetch(`/GetGroupsAndMessages/GetGroupMessages?groupId=${groupId}`)
        .then(response => response.text())
        .then(partialViewHtml => {
            document.querySelector('#chat-container').innerHTML = partialViewHtml;
            setupUploadModalHandlers();
            setupSignalR(groupId, false);
            attachSendButtonListener(groupId, "group");
            setupDocumentButton(groupId); // Add document button functionality
        })
        .catch(error => console.error('Error loading group messages:', error));
}

function openChatWithFriend(friendId, friendName) {
    fetch(`/GetGroupsAndMessages/GetFriendsMessages?friendId=${friendId}`)
        .then(response => response.text())
        .then(partialViewHtml => {
            document.querySelector('#chat-container').innerHTML = partialViewHtml;

            setTimeout(() => {
                const container = document.getElementById("messages-container");
                if (container) {
                    container.scrollTop = container.scrollHeight;
                }
            }, 50);

            setupUploadModalHandlers();
            setupSignalR(friendId, true);
            attachSendButtonListener(friendId, "friends");
            // Friends don't have document functionality as they don't have courseId
        })
        .catch(error => console.error('Error loading friend chat:', error));
}

let connection;
let mediaRecorder;
let audioChunks = [];
let isRecording = false;
let currentChatId = null;
let currentChatType = null;
let selectedFileType = null;
let selectedFile = null; // Store the selected file object
let activeTab = 'documents'; // Track active tab

function setupSignalR(identifier, isPrivate) {
    if (connection) {
        connection.stop();
    }

    currentChatId = identifier;
    currentChatType = isPrivate ? "friends" : "group";

    connection = new signalR.HubConnectionBuilder()
        .withUrl('/Services/ChatHub')
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on('ReceiveMessage', function (user, message, profilePic, fileUrl, fileType, fileName, audioUrl) {
        const chatContainer = document.getElementById('messages-container');
        const currentUser = chatContainer.dataset.currentUser;
        const messageClass = (user === currentUser) ? "sent" : "received";

        let messageContent = '';

        if (message && message.trim() !== '') {
            messageContent += `<p>${message}</p>`;
        }

        if (fileUrl) {
            if (fileType && fileType.startsWith('image/')) {
                messageContent += `
                    <div class="file-preview">
                        <img src="${fileUrl}" alt="${fileName || 'Image'}" class="image-preview" />
                        <a href="${fileUrl}" download="${fileName}" class="file-download">${fileName || 'Download image'}</a>
                    </div>`;
            } else if (fileType && fileType.startsWith('video/')) {
                messageContent += `
                    <div class="file-preview">
                        <video src="${fileUrl}" controls class="video-preview"></video>
                        <a href="${fileUrl}" download="${fileName}" class="file-download">${fileName || 'Download video'}</a>
                    </div>`;
            } else if (fileType && fileType.startsWith('audio/')) {
                messageContent += `
                    <div class="file-preview">
                        <audio src="${fileUrl}" controls class="audio-preview"></audio>
                        <a href="${fileUrl}" download="${fileName}" class="file-download">${fileName || 'Download audio'}</a>
                    </div>`;
            } else {
                messageContent += `
                    <div class="file-preview">
                        <div class="generic-file">
                            <svg class="file-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                                <polyline points="14 2 14 8 20 8"></polyline>
                                <line x1="16" y1="13" x2="8" y2="13"></line>
                                <line x1="16" y1="17" x2="8" y2="17"></line>
                                <line x1="10" y1="9" x2="8" y2="9"></line>
                            </svg>
                            <span class="file-name">${fileName || 'File'}</span>
                        </div>
                        <a href="${fileUrl}" download="${fileName}" class="file-download">Download file</a>
                    </div>`;
            }
        }

        if (audioUrl) {
            messageContent += `
                <div class="file-preview">
                    <audio src="${audioUrl}" controls class="audio-preview"></audio>
                    <a href="${audioUrl}" download="voice-message.wav" class="file-download">Download voice message</a>
                </div>`;
        }

        const newMessage = `
            <div class="message-container ${messageClass}" data-sender="${user}">
                <div class="avatar-wrapper">
                    <img src="${profilePic}" alt="${user}" class="avatar-image"/>
                </div>
                <div class="message-content">
                    <div class="message-bubble">
                        ${messageContent}
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

    connection.start()
        .then(function () {
            console.log("SignalR Connected.");
            connection.invoke("JoinGroup", identifier, isPrivate)
                .catch(function (err) {
                    return console.error(err.toString());
                });
        })
        .catch(function (err) {
            return console.error("SignalR connection error:", err.toString());
        });
}

async function sendMessage(groupId, chatType) {
    const input = document.getElementById('message-input');
    const message = input?.value.trim() || '';

    let fileUrl = null;
    let fileType = null;
    let fileName = null;
    let audioUrl = null;

    // Upload file if one is selected with a file type
    if (selectedFile && selectedFileType) {
        const formData = new FormData();
        formData.append("file", selectedFile);

        try {
            const response = await fetch('/GetGroupsAndMessages/UploadFile', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            if (result.success) {
                fileUrl = result.fileUrl;
                fileType = selectedFileType; // Use the user-selected file type
                fileName = result.fileName;
            } else {
                console.error("File upload failed:", result.message);
                alert("File upload failed: " + result.message);
                return;
            }
        } catch (error) {
            console.error("Error uploading file:", error);
            alert("Error uploading file");
            return;
        }
    }

    const audioDataInput = document.getElementById('recorded-audio-data');
    if (audioDataInput && audioDataInput.value) {
        try {
            const response = await fetch('/GetGroupsAndMessages/UploadAudio', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    audioData: audioDataInput.value
                })
            });

            const result = await response.json();
            if (result.success) {
                audioUrl = result.audioUrl;
            } else {
                console.error("Audio upload failed:", result.message);
                alert("Audio upload failed: " + result.message);
                return;
            }
        } catch (error) {
            console.error("Error uploading audio:", error);
            alert("Error uploading audio");
            return;
        }
    }

    if (!message && !fileUrl && !audioUrl) {
        if (!message) {
            alert('Please provide a message, file, or audio recording.');
        }
        return;
    }

    if (!connection) {
        console.error("SignalR connection not established");
        return;
    }

    connection.invoke('SendMessage', chatType, groupId, message, fileUrl, fileType, fileName, audioUrl)
        .then(() => {
            console.log("Message sent successfully");
            if (input) input.value = '';

            // Clear file selection
            selectedFile = null;
            selectedFileType = null;
            const fileInput = document.getElementById('file-input');
            if (fileInput) fileInput.value = '';

            if (audioDataInput) audioDataInput.value = '';

            const audioPlayer = document.getElementById('audio-player-container');
            if (audioPlayer) audioPlayer.style.display = 'none';

            const filePreview = document.getElementById('file-preview');
            if (filePreview) {
                filePreview.innerHTML = '';
                filePreview.style.display = 'none';
            }

            isRecording = false;
            audioChunks = [];
            updateRecordingUI();
        })
        .catch(err => {
            console.error('SendMessage error:', err);
            alert("Failed to send message. Please try again.");
        });
}

function attachSendButtonListener(identifier, chatType) {
    console.log("Attaching send button listener for:", identifier, chatType);

    currentChatId = identifier;
    currentChatType = chatType;

    const sendButton = document.getElementById("send-button");
    const messageInput = document.getElementById("message-input");

    if (sendButton) {
        const newSendButton = sendButton.cloneNode(true);
        sendButton.parentNode.replaceChild(newSendButton, sendButton);

        newSendButton.addEventListener('click', async function (e) {
            e.preventDefault();
            console.log("Send button clicked");
            await sendMessage(identifier, chatType);
        });
    }

    if (messageInput) {
        const newMessageInput = messageInput.cloneNode(true);
        messageInput.parentNode.replaceChild(newMessageInput, messageInput);

        newMessageInput.addEventListener('keypress', async function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                console.log("Enter key pressed");
                await sendMessage(identifier, chatType);
            }
        });
    }

    const recordButton = document.getElementById('record-button');
    if (recordButton) {
        const newRecordButton = recordButton.cloneNode(true);
        recordButton.parentNode.replaceChild(newRecordButton, recordButton);

        newRecordButton.addEventListener('click', toggleRecording);
    }
}

// Document Panel Functions
function setupDocumentButton(groupId) {
    const documentButton = document.getElementById('document-button');
    if (documentButton) {
        // Remove existing listeners by cloning
        const newDocumentButton = documentButton.cloneNode(true);
        documentButton.parentNode.replaceChild(newDocumentButton, documentButton);

        newDocumentButton.addEventListener('click', function () {
            loadDocuments(groupId);
        });
    }
}


function loadDocuments(groupId) {
    console.log("Loading documents for group:", groupId);

    // Show the panel and display loading state
    openDocumentPanel();

    const contentDiv = document.getElementById('documentPanelContent');
    contentDiv.innerHTML = `
        <div class="loading-state">
            <div class="loading-spinner"></div>
            <p>Loading documents...</p>
        </div>
    `;

    fetch(`/GetGroupsAndMessages/GetDocuments?groupId=${groupId}`)
        .then(response => response.json())
        .then(data => {
            console.log("Documents response:", data);

            if (data.success) {
                allDocuments = data.documents || [];
                allPastPapers = data.pastPapers || []; // Note: Backend has typo "PastPaprs"
                activeTab = 'documents'; // Reset to default tab
                displayDocumentsWithTabs(allDocuments, allPastPapers);
                setupTabFunctionality();
                setupSearchFunctionality();
            } else {
                contentDiv.innerHTML = `
                    <div class="empty-state">
                        <p>❌ ${data.message}</p>
                    </div>
                `;
            }
        })
        .catch(error => {
            console.error('Error loading documents:', error);
            contentDiv.innerHTML = `
                <div class="empty-state">
                    <p>❌ Error loading documents. Please try again.</p>
                </div>
            `;
        });
}


function setupTabFunctionality() {
    const documentsTab = document.getElementById('documents-tab');
    const pastPapersTab = document.getElementById('past-papers-tab');

    if (documentsTab) {
        documentsTab.addEventListener('click', () => switchTab('documents'));
    }
    if (pastPapersTab) {
        pastPapersTab.addEventListener('click', () => switchTab('past-papers'));
    }
}

function switchTab(tabName) {
    activeTab = tabName;

    // Update tab appearance
    const documentsTab = document.getElementById('documents-tab');
    const pastPapersTab = document.getElementById('past-papers-tab');
    const documentsContent = document.getElementById('documents-content');
    const pastPapersContent = document.getElementById('past-papers-content');

    if (tabName === 'documents') {
        documentsTab.classList.add('active');
        pastPapersTab.classList.remove('active');
        documentsContent.style.display = 'flex';
        pastPapersContent.style.display = 'none';
    } else {
        documentsTab.classList.remove('active');
        pastPapersTab.classList.add('active');
        documentsContent.style.display = 'none';
        pastPapersContent.style.display = 'flex';
    }
}

function setupSearchFunctionality() {
    const documentsSearchInput = document.getElementById('documents-search');
    const pastPapersSearchInput = document.getElementById('past-papers-search');

    if (documentsSearchInput) {
        documentsSearchInput.addEventListener('input', function (e) {
            const searchTerm = e.target.value.toLowerCase();
            const filteredDocuments = allDocuments.filter(doc => {
                const fileName = (doc.FileName || doc.fileName || '').toLowerCase();
                return fileName.includes(searchTerm);
            });
            updateDocumentsGrid(filteredDocuments, 'documents-grid');
        });
    }

    if (pastPapersSearchInput) {
        pastPapersSearchInput.addEventListener('input', function (e) {
            const searchTerm = e.target.value.toLowerCase();
            const filteredPastPapers = allPastPapers.filter(paper => {
                const fileName = (paper.FileName || paper.fileName || '').toLowerCase();
                return fileName.includes(searchTerm);
            });
            updateDocumentsGrid(filteredPastPapers, 'past-papers-grid');
        });
    }
}

function updateDocumentsGrid(items, gridId) {
    const grid = document.getElementById(gridId);
    if (!grid) return;

    let html = '';
    if (items.length > 0) {
        const type = gridId === 'documents-grid' ? 'document' : 'past-paper';
        items.forEach(item => {
            html += createDocumentItem(item, type);
        });
    } else {
        html = '<div class="empty-state">No files found matching your search</div>';
    }
    grid.innerHTML = html;
}

function displayDocumentsWithTabs(documents, pastPapers) {
    const contentDiv = document.getElementById('documentPanelContent');

    let html = `
        <div class="document-tabs">
            <button id="documents-tab" class="tab-button active">
                <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
                </svg>
                Documents (${documents.length})
            </button>
            <button id="past-papers-tab" class="tab-button">
                <svg width="16" height="16" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path>
                </svg>
                Past Papers (${pastPapers.length})
            </button>
        </div>

        <div class="tab-content">
            <div id="documents-content" class="document-section" style="display: flex;">
                <div class="search-container">
                    <input type="text" id="documents-search" placeholder="Search documents..." class="search-input">
                </div>
                <div class="document-grid" id="documents-grid">
    `;

    if (documents.length > 0) {
        documents.forEach(doc => {
            html += createDocumentItem(doc, 'document');
        });
    } else {
        html += '<div class="empty-state">No documents available</div>';
    }

    html += `
                </div>
            </div>

            <div id="past-papers-content" class="document-section" style="display: none;">
                <div class="search-container">
                    <input type="text" id="past-papers-search" placeholder="Search past papers..." class="search-input">
                </div>
                <div class="document-grid" id="past-papers-grid">
    `;

    if (pastPapers.length > 0) {
        pastPapers.forEach(paper => {
            html += createDocumentItem(paper, 'past-paper');
        });
    } else {
        html += '<div class="empty-state">No past papers available</div>';
    }

    html += `
                </div>
            </div>
        </div>
    `;

    contentDiv.innerHTML = html;
}

function createDocumentItem(file, type) {
    const iconSvg = type === 'past-paper'
        ? `<svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
             <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path>
           </svg>`
        : `<svg width="20" height="20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
             <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path>
           </svg>`;

    const fileName = file.FileName || file.fileName || 'Unknown file';
    const fileSize = file.FileSize ? formatFileSize(file.FileSize) : '';
    const uploadDate = file.UploadDate ? new Date(file.UploadDate).toLocaleDateString() : '';
    const fileUrl = file.FileUrl || file.fileUrl || '#';

    return `
        <div class="document-item" onclick="window.open('${fileUrl}', '_blank')">
            <div class="document-icon">
                ${iconSvg}
            </div>
            <div class="document-info">
                <div class="document-name">${fileName}</div>
                <div class="document-meta">
                    ${fileSize ? `<span>${fileSize}</span>` : ''}
                    ${uploadDate ? `<span>${uploadDate}</span>` : ''}
                </div>
            </div>
            <a href="${fileUrl}" download="${fileName}" class="document-download-btn" onclick="event.stopPropagation()">
                Download
            </a>
        </div>
    `;
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}

function openDocumentPanel() {
    const panel = document.getElementById('documentPanel');
    panel.classList.add('show');
}

function closeDocumentPanel() {
    const panel = document.getElementById('documentPanel');
    panel.classList.remove('show');
}

// ... keep existing code (Recording functions)
async function toggleRecording() {
    if (!isRecording) {
        await startRecording();
    } else {
        await stopRecording();
    }
}

async function startRecording() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

        mediaRecorder = new MediaRecorder(stream);
        audioChunks = [];

        mediaRecorder.addEventListener('dataavailable', event => {
            if (event.data.size > 0) audioChunks.push(event.data);
        });

        mediaRecorder.addEventListener('stop', () => {
            const audioBlob = new Blob(audioChunks, { type: 'audio/wav' });
            const audioUrl = URL.createObjectURL(audioBlob);

            const audioPlayerContainer = document.getElementById('audio-player-container');
            if (audioPlayerContainer) {
                audioPlayerContainer.innerHTML = `
                    <audio src="${audioUrl}" controls id="audio-preview"></audio>
                    <button type="button" onclick="clearRecording()" class="clear-recording">&times;</button>
                `;
                audioPlayerContainer.style.display = 'block';
            }

            const reader = new FileReader();
            reader.readAsDataURL(audioBlob);
            reader.onloadend = function () {
                const base64data = reader.result;
                const audioDataInput = document.getElementById('recorded-audio-data');
                if (audioDataInput) {
                    audioDataInput.value = base64data;
                }
            };

            stream.getTracks().forEach(track => track.stop());
        });

        mediaRecorder.start();
        isRecording = true;
        updateRecordingUI();
    } catch (error) {
        console.error("Error starting recording:", error);
        alert("Could not access microphone. Please check permissions.");
    }
}

async function stopRecording() {
    if (mediaRecorder && mediaRecorder.state !== 'inactive') {
        mediaRecorder.stop();
        isRecording = false;
        updateRecordingUI();
    }
}

function updateRecordingUI() {
    const recordButton = document.getElementById('record-button');
    const recordIcon = document.getElementById('record-icon');
    const stopIcon = document.getElementById('stop-icon');

    if (recordButton && recordIcon && stopIcon) {
        if (isRecording) {
            recordButton.classList.add('recording');
            recordIcon.style.display = 'none';
            stopIcon.style.display = 'inline';
        } else {
            recordButton.classList.remove('recording');
            recordIcon.style.display = 'inline';
            stopIcon.style.display = 'none';
        }
    }
}

function clearRecording() {
    const audioPlayerContainer = document.getElementById('audio-player-container');
    const audioDataInput = document.getElementById('recorded-audio-data');

    if (audioPlayerContainer) {
        audioPlayerContainer.innerHTML = '';
        audioPlayerContainer.style.display = 'none';
    }

    if (audioDataInput) {
        audioDataInput.value = '';
    }
}

// ... keep existing code (Upload Modal functions)
function setupUploadModalHandlers() {
    const fileBtn = document.querySelector('.message-action-btn');
    if (fileBtn) {
        fileBtn.removeAttribute('onclick');
        fileBtn.addEventListener('click', function () {
            const fileInput = document.getElementById('file-input');
            if (fileInput) {
                fileInput.click();
            }
        });
    }

    const fileInput = document.getElementById('file-input');
    if (fileInput) {
        fileInput.addEventListener('change', handleFileSelection);
    }

    // Set up upload option click handlers
    document.querySelectorAll('.upload-option').forEach(option => {
        option.addEventListener('click', () => {
            console.log('Upload option clicked:', option.getAttribute('data-type'));
            document.querySelectorAll('.upload-option').forEach(opt => opt.classList.remove('selected'));
            option.classList.add('selected');
            selectedFileType = option.getAttribute('data-type');
            console.log('Selected file type set to:', selectedFileType);
            const uploadBtn = document.getElementById('uploadBtn');
            if (uploadBtn) {
                uploadBtn.disabled = false;
            }
        });
    });

    const modal = document.getElementById('uploadModal');
    if (modal) {
        modal.addEventListener('click', e => {
            if (e.target === modal) closeUploadModal();
        });
    }
}

// Modified to only store file and show modal - no upload yet
function handleFileSelection() {
    const fileInput = document.getElementById('file-input');
    if (!fileInput.files || !fileInput.files.length) {
        return;
    }

    // Store the selected file
    selectedFile = fileInput.files[0];
    console.log('File selected:', selectedFile.name);

    // Show the upload modal for file type selection
    openUploadModal();
}

function openUploadModal() {
    const modal = document.getElementById('uploadModal');
    modal.style.display = 'block';
    selectedFileType = null;
    const uploadBtn = document.getElementById('uploadBtn');
    if (uploadBtn) {
        uploadBtn.disabled = true;
    }

    document.querySelectorAll('.upload-option').forEach(el => {
        el.classList.remove('selected');
    });

    // Re-setup the event listeners every time the modal opens
    setupUploadOptionHandlers();
}

function setupUploadOptionHandlers() {
    document.querySelectorAll('.upload-option').forEach(option => {
        // Remove existing listeners by cloning
        const newOption = option.cloneNode(true);
        option.parentNode.replaceChild(newOption, option);

        newOption.addEventListener('click', () => {
            console.log('Upload option clicked:', newOption.getAttribute('data-type'));
            document.querySelectorAll('.upload-option').forEach(opt => opt.classList.remove('selected'));
            newOption.classList.add('selected');
            selectedFileType = newOption.getAttribute('data-type');
            console.log('Selected file type set to:', selectedFileType);
            const uploadBtn = document.getElementById('uploadBtn');
            if (uploadBtn) {
                uploadBtn.disabled = false;
            }
        });
    });
}

function closeUploadModal() {
    const modal = document.getElementById('uploadModal');
    modal.style.display = 'none';
    //selectedFileType = null;

    // Don't clear selectedFile here - keep it for sending message
    // Only clear the file input to allow reselection if needed
    const fileInput = document.getElementById('file-input');
    if (fileInput) fileInput.value = '';
}

function handleFileUpload() {
    if (!selectedFileType || !selectedFile) {
        alert('Please select a file type');
        return;
    }

    // Just close the modal - the file will be uploaded when sendMessage is called
    closeUploadModal();

    // Show some indication that file is ready to send
    console.log('File ready to send:', selectedFile.name, 'Type:', selectedFileType);

    // Optional: Show a visual indicator that a file is ready to be sent
    const filePreview = document.getElementById('file-preview');
    if (filePreview) {
        filePreview.innerHTML = `
            <div class="file-ready-indicator">
                <span>📎 File ready: ${selectedFile.name} (${selectedFileType})</span>
                <button onclick="clearSelectedFile()" class="clear-file-btn">×</button>
            </div>
        `;
        filePreview.style.display = 'block';
    }
}

// Add a function to clear the selected file if user wants to cancel
function clearSelectedFile() {
    selectedFile = null;
    selectedFileType = null;
    const filePreview = document.getElementById('file-preview');
    if (filePreview) {
        filePreview.innerHTML = '';
        filePreview.style.display = 'none';
    }
    const fileInput = document.getElementById('file-input');
    if (fileInput) fileInput.value = '';
}
