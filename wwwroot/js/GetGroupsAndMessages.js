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
                                    <div class="avatar-placeholder"><img src="/images/groupLogo.jpg" alt="Logo"></div>
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

            // FIXED: Process loaded messages to fix video display
            processLoadedMessages();

            setupUploadModalHandlers();
            setupSignalR(groupId, false);
            attachSendButtonListener(groupId, "group");
            setupDocumentButton(groupId);
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

            // FIXED: Process loaded messages to fix video display
            processLoadedMessages();

            setupUploadModalHandlers();
            setupSignalR(friendId, true);
            attachSendButtonListener(friendId, "friends");
        })
        .catch(error => console.error('Error loading friend chat:', error));
}

// FIXED: New function to process loaded messages and fix video/media display
function processLoadedMessages() {
    setTimeout(() => {
        const messagesContainer = document.getElementById('messages-container');
        if (!messagesContainer) return;

        // Find all message containers with file attachments
        const messageContainers = messagesContainer.querySelectorAll('.message-container');

        messageContainers.forEach(messageContainer => {
            // Look for file links or document previews that should be videos
            const fileLinks = messageContainer.querySelectorAll('a[href*="/uploads/"]');
            const documentPreviews = messageContainer.querySelectorAll('.document-preview, .generic-file-container, .file-preview');

            fileLinks.forEach(link => {
                const fileUrl = link.href;
                const fileName = link.textContent || link.download || 'file';

                // Try to determine file type from URL or filename
                const fileInfo = getFileTypeInfo(null, fileName);

                if (fileInfo.type === 'video') {
                    // Replace document link with video player
                    const videoContainer = createVideoPreview(fileUrl, fileName, fileInfo);
                    link.parentNode.replaceChild(videoContainer, link);
                } else if (fileInfo.type === 'image') {
                    // Replace document link with image preview
                    const imageContainer = createImagePreview(fileUrl, fileName, fileInfo);
                    link.parentNode.replaceChild(imageContainer, link);
                } else if (fileInfo.type === 'audio') {
                    // Replace document link with audio player
                    const audioContainer = createAudioPreview(fileUrl, fileName, fileInfo);
                    link.parentNode.replaceChild(audioContainer, link);
                }
            });

            // Also check existing document previews to see if they should be videos/images/audio
            documentPreviews.forEach(preview => {
                const fileNameElement = preview.querySelector('.file-name, .document-name');
                const downloadLink = preview.querySelector('a[download], a[href*="/uploads/"]');

                if (fileNameElement && downloadLink) {
                    const fileName = fileNameElement.textContent;
                    const fileUrl = downloadLink.href;
                    const fileInfo = getFileTypeInfo(null, fileName);

                    if (fileInfo.type === 'video') {
                        // Replace document preview with video player
                        const videoContainer = createVideoPreview(fileUrl, fileName, fileInfo);
                        preview.parentNode.replaceChild(videoContainer, preview);
                    } else if (fileInfo.type === 'image') {
                        // Replace document preview with image preview
                        const imageContainer = createImagePreview(fileUrl, fileName, fileInfo);
                        preview.parentNode.replaceChild(imageContainer, preview);
                    } else if (fileInfo.type === 'audio') {
                        // Replace document preview with audio player
                        const audioContainer = createAudioPreview(fileUrl, fileName, fileInfo);
                        preview.parentNode.replaceChild(audioContainer, preview);
                    }
                }
            });
        });
    }, 100); // Small delay to ensure DOM is fully loaded
}

// FIXED: Helper function to create video preview container
function createVideoPreview(fileUrl, fileName, fileInfo) {
    const container = document.createElement('div');
    container.className = 'file-preview video-preview-container';
    container.innerHTML = `
        <video src="${fileUrl}" controls class="video-preview" preload="metadata">
            <source src="${fileUrl}" type="video/mp4">
            Your browser does not support the video tag.
        </video>
        <div class="file-info">
            <span class="file-name">${fileName}</span>
            <span class="file-category">${fileInfo.category}</span>
            <a href="${fileUrl}" download="${fileName}" class="file-download">Download Video</a>
        </div>
    `;
    return container;
}

// FIXED: Helper function to create image preview container
function createImagePreview(fileUrl, fileName, fileInfo) {
    const container = document.createElement('div');
    container.className = 'file-preview image-preview-container';
    container.innerHTML = `
        <img src="${fileUrl}" alt="${fileName}" class="image-preview" onclick="openImageModal('${fileUrl}', '${fileName}')" loading="lazy" />
        <div class="file-info">
            <span class="file-name">${fileName}</span>
            <a href="${fileUrl}" download="${fileName}" class="file-download">Download</a>
        </div>
    `;
    return container;
}

// FIXED: Helper function to create audio preview container
function createAudioPreview(fileUrl, fileName, fileInfo) {
    const container = document.createElement('div');
    container.className = 'file-preview audio-preview-container';
    container.innerHTML = `
        <audio src="${fileUrl}" controls class="audio-preview" preload="metadata">
            <source src="${fileUrl}" type="audio/mpeg">
            Your browser does not support the audio tag.
        </audio>
        <div class="file-info">
            <span class="file-name">${fileName}</span>
            <span class="file-category">${fileInfo.category}</span>
            <a href="${fileUrl}" download="${fileName}" class="file-download">Download Audio</a>
        </div>
    `;
    return container;
}

let mediaRecorder;
let audioChunks = [];
let isRecording = false;
let currentChatId = null;
let currentChatType = null;
let selectedFileType = null;
let selectedFile = null;
let uploadedFileData = null; // Separate variable to track uploaded file data
let activeTab = 'documents';
let allDocuments = [];
let allPastPapers = [];
let uploadXHR = null; // For tracking upload progress and enabling cancellation

// Enhanced file type detection and handling
function getFileTypeInfo(fileType, fileName) {
    const extension = fileName ? fileName.split('.').pop().toLowerCase() : '';

    // Video formats
    const videoFormats = ['mp4', 'webm', 'ogg', 'avi', 'mov', 'wmv', 'flv', 'mkv', '3gp', 'm4v'];
    // Image formats
    const imageFormats = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp', 'svg', 'ico', 'tiff'];
    // Audio formats
    const audioFormats = ['mp3', 'wav', 'ogg', 'aac', 'flac', 'm4a', 'wma'];
    // Document formats
    const documentFormats = ['pdf', 'doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx', 'txt', 'rtf', 'odt', 'ods', 'odp'];
    // Archive formats
    const archiveFormats = ['zip', 'rar', '7z', 'tar', 'gz', 'bz2'];
    // Code formats
    const codeFormats = ['js', 'html', 'css', 'php', 'py', 'java', 'cpp', 'c', 'cs', 'xml', 'json'];

    if (fileType) {
        if (fileType.startsWith('video/')) return { type: 'video', category: 'Video' };
        if (fileType.startsWith('image/')) return { type: 'image', category: 'Image' };
        if (fileType.startsWith('audio/')) return { type: 'audio', category: 'Audio' };
        if (fileType.includes('pdf')) return { type: 'pdf', category: 'PDF Document' };
        if (fileType.includes('document') || fileType.includes('word')) return { type: 'document', category: 'Document' };
        if (fileType.includes('spreadsheet') || fileType.includes('excel')) return { type: 'spreadsheet', category: 'Spreadsheet' };
        if (fileType.includes('presentation') || fileType.includes('powerpoint')) return { type: 'presentation', category: 'Presentation' };
        if (fileType.includes('zip') || fileType.includes('archive')) return { type: 'archive', category: 'Archive' };
    }

    // Fallback to extension-based detection
    if (videoFormats.includes(extension)) return { type: 'video', category: 'Video' };
    if (imageFormats.includes(extension)) return { type: 'image', category: 'Image' };
    if (audioFormats.includes(extension)) return { type: 'audio', category: 'Audio' };
    if (documentFormats.includes(extension)) return { type: 'document', category: 'Document' };
    if (archiveFormats.includes(extension)) return { type: 'archive', category: 'Archive' };
    if (codeFormats.includes(extension)) return { type: 'code', category: 'Code File' };

    return { type: 'generic', category: 'File' };
}

function getFileIcon(fileInfo, fileName) {
    const extension = fileName ? fileName.split('.').pop().toLowerCase() : '';

    switch (fileInfo.type) {
        case 'video':
            return `<svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polygon points="23 7 16 12 23 17 23 7"></polygon>
                <rect x="1" y="5" width="15" height="14" rx="2" ry="2"></rect>
            </svg>`;
        case 'image':
            return `<svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                <circle cx="8.5" cy="8.5" r="1.5"></circle>
                <polyline points="21,15 16,10 5,21"></polyline>
            </svg>`;
        case 'audio':
            return `<svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M9 18V5l12-2v13"></path>
                <circle cx="6" cy="18" r="3"></circle>
                <circle cx="18" cy="16" r="3"></circle>
            </svg>`;
        case 'pdf':
            return `<svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                <polyline points="14 2 14 8 20 8"></polyline>
                <text x="8" y="16" font-size="6" fill="currentColor">PDF</text>
            </svg>`;
        case 'document':
            if (['doc', 'docx'].includes(extension)) {
                return `<svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                    <polyline points="14 2 14 8 20 8"></polyline>
                    <text x="7" y="16" font-size="5" fill="currentColor">DOC</text>
                </svg>`;
            }
            return `<svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                <polyline points="14 2 14 8 20 8"></polyline>
                <line x1="16" y1="13" x2="8" y2="13"></line>
                <line x1="16" y1="17" x2="8" y2="17"></line>
            </svg>`;
        case 'spreadsheet':
            return `<svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                <polyline points="14 2 14 8 20 8"></polyline>
                <path d="M8 13h8M8 17h8M8 10h2M8 6h2"></path>
            </svg>`;
        case 'presentation':
            return `<svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
                <line x1="8" y1="21" x2="16" y2="21"></line>
                <line x1="12" y1="17" x2="12" y2="21"></line>
            </svg>`;
        case 'archive':
            return `<svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="21 8 21 21 3 21 3 8"></polyline>
                <rect x="1" y="3" width="22" height="5"></rect>
                <line x1="10" y1="12" x2="14" y2="12"></line>
            </svg>`;
        case 'code':
            return `<svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="16 18 22 12 16 6"></polyline>
                <polyline points="8 6 2 12 8 18"></polyline>
            </svg>`;
        default:
            return `<svg class="file-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                <polyline points="14 2 14 8 20 8"></polyline>
                <line x1="16" y1="13" x2="8" y2="13"></line>
                <line x1="16" y1="17" x2="8" y2="17"></line>
                <line x1="10" y1="9" x2="8" y2="9"></line>
            </svg>`;
    }
}

function setupSignalR(identifier, isPrivate) {
    if (connection) {
        connection.stop();
    }

    currentChatId = identifier;
    currentChatType = isPrivate ? "friends" : "group";

    connection = new signalR.HubConnectionBuilder()
        .withUrl('/Services/ChatHub')
        .configureLogging(signalR.LogLevel.Information)
        .withAutomaticReconnect()
        .build();

    connection.on('ReceiveMessage', function (user, message, profilePic, fileUrl, fileType, fileName, audioUrl) {
        const chatContainer = document.getElementById('messages-container');
        const currentUser = chatContainer.dataset.currentUser;
        const messageClass = (user === currentUser) ? "sent" : "received";

        let messageContent = '';

        if (message && message.trim() !== '') {
            messageContent += `<p>${message}</p>`;
        }

        if (fileUrl && fileName) {
            const fileInfo = getFileTypeInfo(fileType, fileName);
            const fileSize = selectedFile ? formatFileSize(selectedFile.size) : '';

            // FIXED: Properly handle video files to show video player instead of document
            if (fileInfo.type === 'video') {
                messageContent += `
                    <div class="file-preview video-preview-container">
                        <video src="${fileUrl}" controls class="video-preview" preload="metadata">
                            <source src="${fileUrl}" type="${fileType || 'video/mp4'}">
                            Your browser does not support the video tag.
                        </video>
                        <div class="file-info">
                            <span class="file-name">${fileName}</span>
                            <span class="file-category">${fileInfo.category}</span>
                            <a href="${fileUrl}" download="${fileName}" class="file-download">Download Video</a>
                        </div>
                    </div>`;
            } else if (fileInfo.type === 'image') {
                messageContent += `
                    <div class="file-preview image-preview-container">
                        <img src="${fileUrl}" alt="${fileName}" class="image-preview" onclick="openImageModal('${fileUrl}', '${fileName}')" loading="lazy" />
                        <div class="file-info">
                            <span class="file-name">${fileName}</span>
                            <a href="${fileUrl}" download="${fileName}" class="file-download">Download</a>
                        </div>
                    </div>`;
            } else if (fileInfo.type === 'audio') {
                messageContent += `
                    <div class="file-preview audio-preview-container">
                        <audio src="${fileUrl}" controls class="audio-preview" preload="metadata">
                            <source src="${fileUrl}" type="${fileType || 'audio/mpeg'}">
                            Your browser does not support the audio tag.
                        </audio>
                        <div class="file-info">
                            <span class="file-name">${fileName}</span>
                            <span class="file-category">${fileInfo.category}</span>
                            <a href="${fileUrl}" download="${fileName}" class="file-download">Download Audio</a>
                        </div>
                    </div>`;
            } else if (fileInfo.type === 'pdf') {
                const fileIcon = getFileIcon(fileInfo, fileName);
                messageContent += `
                    <div class="file-preview document-preview-container">
                        <div class="document-preview">
                            <iframe src="${fileUrl}" class="pdf-preview" title="${fileName}"></iframe>
                            <div class="pdf-fallback">
                                <div class="generic-file">
                                    ${fileIcon}
                                    <div class="file-details">
                                        <span class="file-name">${fileName}</span>
                                        <span class="file-category">${fileInfo.category}</span>
                                        ${fileSize ? `<span class="file-size">${fileSize}</span>` : ''}
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="file-actions">
                            <button onclick="window.open('${fileUrl}', '_blank')" class="file-view">View PDF</button>
                            <a href="${fileUrl}" download="${fileName}" class="file-download">Download</a>
                        </div>
                    </div>`;
            } else {
                const fileIcon = getFileIcon(fileInfo, fileName);
                messageContent += `
                    <div class="file-preview generic-file-container">
                        <div class="generic-file">
                            ${fileIcon}
                            <div class="file-details">
                                <span class="file-name">${fileName}</span>
                                <span class="file-category">${fileInfo.category}</span>
                                ${fileSize ? `<span class="file-size">${fileSize}</span>` : ''}
                            </div>
                        </div>
                        <div class="file-actions">
                            <button onclick="window.open('${fileUrl}', '_blank')" class="file-view">Open</button>
                            <a href="${fileUrl}" download="${fileName}" class="file-download">Download</a>
                        </div>
                    </div>`;
            }
        }

        if (audioUrl) {
            messageContent += `
                <div class="file-preview voice-message-container">
                    <audio src="${audioUrl}" controls class="audio-preview"></audio>
                    <div class="file-info">
                        <span class="file-name">Voice Message</span>
                        <a href="${audioUrl}" download="voice-message.wav" class="file-download">Download</a>
                    </div>
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

    connection.on("receiveannouncement", (title, content, priority) => {
        console.log("New announcement:", title);
        if (typeof announcementCount !== 'undefined') {
            announcementCount++;
            updateAnnouncementBadge();
        }
        if (typeof window.showAnnouncementModal === 'function') {
            window.showAnnouncementModal(title, content, priority);
        }
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

// Image modal for full-screen viewing
function openImageModal(imageUrl, imageName) {
    const modal = document.createElement('div');
    modal.className = 'image-modal';
    modal.innerHTML = `
        <div class="image-modal-content">
            <span class="image-modal-close">&times;</span>
            <img src="${imageUrl}" alt="${imageName}" class="image-modal-img">
            <div class="image-modal-caption">${imageName}</div>
        </div>
    `;

    document.body.appendChild(modal);
    modal.style.display = 'block';

    modal.querySelector('.image-modal-close').onclick = () => {
        document.body.removeChild(modal);
    };

    modal.onclick = (e) => {
        if (e.target === modal) {
            document.body.removeChild(modal);
        }
    };
}

// Enhanced upload progress functions for large files
function showUploadProgress(fileName, fileSize = null) {
    const filePreview = document.getElementById('file-preview');
    if (filePreview) {
        const fileSizeText = fileSize ? ` (${formatFileSize(fileSize)})` : '';
        const isLargeFile = fileSize && fileSize > 50 * 1024 * 1024; // 50MB threshold

        filePreview.innerHTML = `
            <div class="upload-progress-indicator ${isLargeFile ? 'large-file' : ''}">
                <div class="upload-spinner"></div>
                <div class="upload-text">
                    <span>Uploading ${fileName}${fileSizeText}...</span>
                    <small>${isLargeFile ? 'Large file detected - this may take several minutes' : 'Please wait while your file is being uploaded'}</small>
                    <div class="upload-progress-bar" id="upload-progress-bar" style="display: none;">
                        <div class="progress-fill" id="progress-fill"></div>
                        <span class="progress-text" id="progress-text">0%</span>
                    </div>
                    ${isLargeFile ? '<button onclick="cancelUpload()" class="cancel-upload-btn">Cancel Upload</button>' : ''}
                </div>
            </div>
        `;
        filePreview.style.display = 'block';
    }
}

function updateUploadProgress(percentage) {
    const progressBar = document.getElementById('upload-progress-bar');
    const progressFill = document.getElementById('progress-fill');
    const progressText = document.getElementById('progress-text');

    if (progressBar && progressFill && progressText) {
        progressBar.style.display = 'block';
        progressFill.style.width = `${percentage}%`;
        progressText.textContent = `${Math.round(percentage)}%`;
    }
}

function hideUploadProgress() {
    const filePreview = document.getElementById('file-preview');
    if (filePreview) {
        filePreview.innerHTML = '';
        filePreview.style.display = 'none';
    }
}

function cancelUpload() {
    if (uploadXHR) {
        uploadXHR.abort();
        uploadXHR = null;
    }
    hideUploadProgress();
    disableSendButton(false);

    // Clear selected file and uploaded data
    selectedFile = null;
    selectedFileType = null;
    uploadedFileData = null;
    const fileInput = document.getElementById('file-input');
    if (fileInput) fileInput.value = '';

    console.log('Upload cancelled by user');
}

function disableSendButton(disable) {
    const sendButton = document.getElementById('send-button');
    const messageInput = document.getElementById('message-input');
    const recordButton = document.getElementById('record-button');
    const fileButtons = document.querySelectorAll('.message-action-btn');

    if (sendButton) {
        sendButton.disabled = disable;
        if (disable) {
            sendButton.style.opacity = '0.5';
            sendButton.style.cursor = 'not-allowed';
        } else {
            sendButton.style.opacity = '1';
            sendButton.style.cursor = 'pointer';
        }
    }

    if (messageInput) {
        messageInput.disabled = disable;
        if (disable) {
            messageInput.style.opacity = '0.5';
            messageInput.placeholder = 'File uploading, please wait...';
        } else {
            messageInput.style.opacity = '1';
            messageInput.placeholder = 'Type a message...';
        }
    }

    // Disable other action buttons during upload
    if (recordButton) {
        recordButton.disabled = disable;
        recordButton.style.opacity = disable ? '0.5' : '1';
    }

    fileButtons.forEach(btn => {
        btn.disabled = disable;
        btn.style.opacity = disable ? '0.5' : '1';
        btn.style.pointerEvents = disable ? 'none' : 'auto';
    });
}

async function sendMessage(groupId, chatType) {
    const input = document.getElementById('message-input');
    const message = input?.value.trim() || '';

    let fileUrl = null;
    let fileType = null;
    let fileName = null;
    let audioUrl = null;

    // FIXED: Check if file is already uploaded (for large files) using separate uploadedFileData
    if (uploadedFileData && uploadedFileData.fileUrl) {
        fileUrl = uploadedFileData.fileUrl;
        fileType = uploadedFileData.fileType;
        fileName = uploadedFileData.fileName;
        console.log('Using pre-uploaded file:', fileName);
    }
    // Upload file if one is selected with a file type but not yet uploaded
    else if (selectedFile && selectedFileType && !uploadedFileData) {
        console.log('Starting file upload for:', selectedFile.name);

        // Show upload progress and disable send button
        showUploadProgress(selectedFile.name, selectedFile.size);
        disableSendButton(true);

        const formData = new FormData();
        formData.append("file", selectedFile);

        try {
            // Use XMLHttpRequest for progress tracking on large files
            const result = await uploadFileWithProgress(formData, selectedFile.size);

            if (result.success) {
                fileUrl = result.fileUrl;
                fileType = selectedFileType;
                fileName = result.fileName;
                hideUploadProgress();
                console.log('File uploaded successfully during send:', fileName);
            } else {
                console.error("File upload failed:", result.message);
                alert("File upload failed: " + result.message);
                hideUploadProgress();
                disableSendButton(false);
                return;
            }
        } catch (error) {
            if (error.name === 'AbortError' || (error.message && error.message.includes('cancelled'))) {
                console.log('Upload was cancelled');
                return; // Don't show error for user cancellation
            }
            console.error("Error uploading file:", error);
            alert("Error uploading file: " + error.message);
            hideUploadProgress();
            disableSendButton(false);
            return;
        }
    }

    // Handle audio upload
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

    // FIXED: Better validation that properly handles all cases
    const hasMessage = message && message.trim() !== '';
    const hasFile = fileUrl && fileUrl.trim() !== '';
    const hasAudio = audioUrl && audioUrl.trim() !== '';

    console.log('Send validation:', { hasMessage, hasFile, hasAudio, message, fileUrl, audioUrl });

    if (!hasMessage && !hasFile && !hasAudio) {
        alert('Please provide a message, attach a file, or record audio before sending.');
        disableSendButton(false);
        return;
    }

    if (!connection) {
        console.error("SignalR connection not established");
        disableSendButton(false);
        return;
    }

    try {
        await connection.invoke('SendMessage', chatType, groupId, message, fileUrl, fileType, fileName, audioUrl);

        console.log("Message sent successfully");

        // Clear all inputs and states after successful send
        if (input) input.value = '';

        // Clear file-related data
        selectedFile = null;
        selectedFileType = null;
        uploadedFileData = null;
        const fileInput = document.getElementById('file-input');
        if (fileInput) fileInput.value = '';

        // Clear audio data
        if (audioDataInput) audioDataInput.value = '';

        const audioPlayer = document.getElementById('audio-player-container');
        if (audioPlayer) audioPlayer.style.display = 'none';

        // Clear file preview
        const filePreview = document.getElementById('file-preview');
        if (filePreview) {
            filePreview.innerHTML = '';
            filePreview.style.display = 'none';
        }

        // Reset recording state
        isRecording = false;
        audioChunks = [];
        updateRecordingUI();

        // Re-enable send button after successful message
        disableSendButton(false);

    } catch (err) {
        console.error('SendMessage error:', err);
        alert("Failed to send message. Please try again.");
        // Re-enable send button on error
        disableSendButton(false);
    }
}

// Enhanced file upload with progress tracking for large files
function uploadFileWithProgress(formData, fileSize) {
    return new Promise((resolve, reject) => {
        uploadXHR = new XMLHttpRequest();

        // Enhanced timeout for very large files
        const isVeryLargeFile = fileSize > 100 * 1024 * 1024; // 100MB
        const isLargeFile = fileSize > 30 * 1024 * 1024; // 30MB

        if (isVeryLargeFile) {
            uploadXHR.timeout = 900000; // 15 minutes for very large files
        } else if (isLargeFile) {
            uploadXHR.timeout = 600000; // 10 minutes for large files
        } else {
            uploadXHR.timeout = 300000; // 5 minutes for normal files
        }

        uploadXHR.upload.addEventListener('progress', (e) => {
            if (e.lengthComputable) {
                const percentage = (e.loaded / e.total) * 100;
                updateUploadProgress(percentage);

                // Update upload text with speed and ETA for large files
                if (fileSize > 10 * 1024 * 1024) { // 10MB threshold for speed calculation
                    const uploadSpeed = e.loaded / ((Date.now() - uploadXHR.startTime) / 1000);
                    const remainingBytes = e.total - e.loaded;
                    const eta = remainingBytes / uploadSpeed;

                    const speedText = formatFileSize(uploadSpeed) + '/s';
                    const etaText = eta > 60 ? `${Math.round(eta / 60)}m` : `${Math.round(eta)}s`;

                    const uploadText = document.querySelector('.upload-progress-indicator .upload-text span');
                    if (uploadText) {
                        uploadText.innerHTML = `Uploading... ${speedText} • ETA: ${etaText}`;
                    }
                }
            }
        });

        uploadXHR.addEventListener('load', () => {
            if (uploadXHR.status === 200) {
                try {
                    const result = JSON.parse(uploadXHR.responseText);
                    resolve(result);
                } catch (e) {
                    reject(new Error('Invalid server response'));
                }
            } else if (uploadXHR.status === 413) {
                reject(new Error('File too large. Please try a smaller file.'));
            } else if (uploadXHR.status === 408) {
                reject(new Error('Upload timeout. Please try again or use a smaller file.'));
            } else {
                reject(new Error(`Upload failed with status: ${uploadXHR.status}`));
            }
        });

        uploadXHR.addEventListener('error', () => {
            reject(new Error('Network error during upload. Please check your connection and try again.'));
        });

        uploadXHR.addEventListener('timeout', () => {
            reject(new Error('Upload timeout - please try again with a smaller file or check your connection'));
        });

        uploadXHR.addEventListener('abort', () => {
            const abortError = new Error('Upload was cancelled');
            abortError.name = 'AbortError';
            reject(abortError);
        });

        uploadXHR.open('POST', '/GetGroupsAndMessages/UploadFile');
        uploadXHR.startTime = Date.now();
        uploadXHR.send(formData);
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

function setupDocumentButton(groupId) {
    const documentButton = document.getElementById('document-button');
    if (documentButton) {
        const newDocumentButton = documentButton.cloneNode(true);
        documentButton.parentNode.replaceChild(newDocumentButton, documentButton);

        newDocumentButton.addEventListener('click', function () {
            loadDocuments(groupId);
        });
    }
}

function loadDocuments(groupId) {
    console.log("Loading documents for group:", groupId);

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
                allPastPapers = data.pastPapers || [];
                activeTab = 'documents';
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
    const fileName = file.FileName || file.fileName || 'Unknown file';
    const fileUrl = file.FileUrl || file.fileUrl || '#';
    const fileInfo = getFileTypeInfo(file.FileType || file.fileType, fileName);
    const fileIcon = getFileIcon(fileInfo, fileName);
    const fileSize = file.FileSize ? formatFileSize(file.FileSize) : '';
    const uploadDate = file.UploadDate ? new Date(file.UploadDate).toLocaleDateString() : '';

    return `
        <div class="document-item" onclick="window.open('${fileUrl}', '_blank')">
            <div class="document-icon">
                ${fileIcon}
            </div>
            <div class="document-info">
                <div class="document-name">${fileName}</div>
                <div class="document-meta">
                    <span class="file-category">${fileInfo.category}</span>
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
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}

function openDocumentPanel() {
    const panel = document.getElementById('documentPanel');
    if (panel) panel.classList.add('show');
}

function closeDocumentPanel() {
    const panel = document.getElementById('documentPanel');
    if (panel) panel.classList.remove('show');
}

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

// FIXED: Always show upload modal regardless of file size
function handleFileSelection() {
    const fileInput = document.getElementById('file-input');
    if (!fileInput.files || !fileInput.files.length) {
        return;
    }

    selectedFile = fileInput.files[0];
    console.log('File selected:', selectedFile.name, 'Size:', formatFileSize(selectedFile.size));

    // Clear any previous uploaded file data
    uploadedFileData = null;

    // Always show the upload modal for category selection
    openUploadModal();
}

function openUploadModal() {
    const modal = document.getElementById('uploadModal');
    if (!modal) return;

    modal.style.display = 'flex';
    selectedFileType = null;
    const uploadBtn = document.getElementById('uploadBtn');
    if (uploadBtn) {
        uploadBtn.disabled = true;
    }

    document.querySelectorAll('.upload-option').forEach(el => {
        el.classList.remove('selected');
    });

    // Enhanced upload modal with more file type options
    const modalContent = modal.querySelector('.upload-options') || modal.querySelector('.modal-content');
    if (modalContent && selectedFile) {
        const detectedInfo = getFileTypeInfo(selectedFile.type, selectedFile.name);
        const fileSize = formatFileSize(selectedFile.size);
        const isLargeFile = selectedFile.size > 50 * 1024 * 1024; // 50MB

        // Add file preview to modal
        const filePreviewHtml = `
            <div class="selected-file-preview ${isLargeFile ? 'large-file-warning' : ''}">
                <h4>Selected File:</h4>
                <div class="file-info">
                    <span class="file-name">${selectedFile.name}</span>
                    <span class="file-size">${fileSize}</span>
                    <span class="detected-type">Detected: ${detectedInfo.category}</span>
                    ${isLargeFile ? '<span class="large-file-notice">⚠️ Large file - upload may take several minutes</span>' : ''}
                </div>
            </div>
        `;

        // Insert at the beginning of modal content
        modalContent.insertAdjacentHTML('afterbegin', filePreviewHtml);
    }

    setupUploadOptionHandlers();
}

function setupUploadOptionHandlers() {
    document.querySelectorAll('.upload-option').forEach(option => {
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
    if (modal) {
        modal.style.display = 'none';

        // Remove file preview
        const filePreview = modal.querySelector('.selected-file-preview');
        if (filePreview) {
            filePreview.remove();
        }
    }

    const fileInput = document.getElementById('file-input');
    if (fileInput) fileInput.value = '';
}

function handleFileUpload() {
    if (!selectedFileType || !selectedFile) {
        alert('Please select a file type');
        return;
    }

    closeUploadModal();

    console.log('File ready to send:', selectedFile.name, 'Type:', selectedFileType);

    // For large files (>30MB), start upload immediately and show progress
    const isLargeFile = selectedFile.size > 30 * 1024 * 1024; // 30MB threshold

    if (isLargeFile) {
        // Start uploading immediately for large files
        startFileUpload();
    } else {
        // For smaller files, just show ready indicator
        showFileReadyIndicator();
    }
}

// Show file ready indicator for files that haven't been uploaded yet
function showFileReadyIndicator() {
    const filePreview = document.getElementById('file-preview');
    if (filePreview) {
        const fileInfo = getFileTypeInfo(selectedFileType || selectedFile.type, selectedFile.name);
        const fileIcon = getFileIcon(fileInfo, selectedFile.name);
        const fileSize = formatFileSize(selectedFile.size);

        filePreview.innerHTML = `
            <div class="file-ready-indicator">
                <div class="file-preview-content">
                    ${fileIcon}
                    <div class="file-details">
                        <span class="file-name">${selectedFile.name}</span>
                        <span class="file-info">${fileInfo.category} • ${fileSize}</span>
                    </div>
                </div>
                <button onclick="clearSelectedFile()" class="clear-file-btn">×</button>
            </div>
        `;
        filePreview.style.display = 'block';
    }
}

// FIXED: New function to start file upload immediately for large files
async function startFileUpload() {
    if (!selectedFile || !selectedFileType) {
        return;
    }

    console.log('Starting immediate upload for large file:', selectedFile.name);

    // Show upload progress and disable send button
    showUploadProgress(selectedFile.name, selectedFile.size);
    disableSendButton(true);

    const formData = new FormData();
    formData.append("file", selectedFile);

    try {
        // Use XMLHttpRequest for progress tracking
        const result = await uploadFileWithProgress(formData, selectedFile.size);

        if (result.success) {
            // FIXED: Store the uploaded file info in separate variable
            uploadedFileData = {
                fileUrl: result.fileUrl,
                fileType: selectedFileType,
                fileName: result.fileName,
                originalFile: selectedFile
            };

            hideUploadProgress();
            showUploadedFileIndicator();
            disableSendButton(false);

            console.log('Large file uploaded successfully:', result.fileName);
        } else {
            console.error("File upload failed:", result.message);
            alert("File upload failed: " + result.message);
            hideUploadProgress();
            disableSendButton(false);
            clearSelectedFile();
        }
    } catch (error) {
        if (error.message && (error.message.includes('cancelled') || error.name === 'AbortError')) {
            console.log('Upload was cancelled');
            return;
        }
        console.error("Error uploading file:", error);
        alert("Error uploading file: " + error.message);
        hideUploadProgress();
        disableSendButton(false);
        clearSelectedFile();
    }
}

// FIXED: New function to show uploaded file ready indicator
function showUploadedFileIndicator() {
    const filePreview = document.getElementById('file-preview');
    if (filePreview && uploadedFileData) {
        const fileInfo = getFileTypeInfo(uploadedFileData.fileType, uploadedFileData.fileName);
        const fileIcon = getFileIcon(fileInfo, uploadedFileData.fileName);
        const fileSize = uploadedFileData.originalFile ? formatFileSize(uploadedFileData.originalFile.size) : '';

        filePreview.innerHTML = `
            <div class="file-uploaded-indicator">
                <div class="file-preview-content">
                    <div class="upload-success-badge">✓ Uploaded</div>
                    ${fileIcon}
                    <div class="file-details">
                        <span class="file-name">${uploadedFileData.fileName}</span>
                        <span class="file-info">${fileInfo.category} • ${fileSize}</span>
                        <span class="upload-status">Ready to send</span>
                    </div>
                </div>
                <button onclick="clearSelectedFile()" class="clear-file-btn">×</button>
            </div>
        `;
        filePreview.style.display = 'block';
    }
}

function clearSelectedFile() {
    // Cancel ongoing upload if any
    if (uploadXHR) {
        uploadXHR.abort();
        uploadXHR = null;
    }

    // Clear all file-related data
    selectedFile = null;
    selectedFileType = null;
    uploadedFileData = null;

    const filePreview = document.getElementById('file-preview');
    if (filePreview) {
        filePreview.innerHTML = '';
        filePreview.style.display = 'none';
    }

    const fileInput = document.getElementById('file-input');
    if (fileInput) fileInput.value = '';

    // Re-enable controls
    hideUploadProgress();
    disableSendButton(false);

    console.log('Selected file cleared');
}