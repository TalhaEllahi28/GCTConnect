// Sample data for demonstration
let posts = [
    {
        id: 1,
        author: 'Mike Chen',
        department: 'Computer Science, 2024',
        avatar: 'https://images.pexels.com/photos/1547971/pexels-photo-1547971.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
        content: 'Just finished my Machine Learning project! The AI can now recognize different types of campus buildings with 95% accuracy. Excited to present this next week! 🤖',
        media: 'https://images.pexels.com/photos/3861958/pexels-photo-3861958.jpeg?auto=compress&cs=tinysrgb&w=600&h=300&fit=crop',
        privacy: 'Public',
        likes: 24,
        comments: 8,
        shares: 3,
        liked: false,
        timestamp: '2 hours ago'
    },
    {
        id: 2,
        author: 'Emma Rodriguez',
        department: 'Psychology, 2025',
        avatar: 'https://images.pexels.com/photos/1542085/pexels-photo-1542085.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
        content: 'Study group for Cognitive Psychology tomorrow at 3 PM in the library! We\'ll be covering chapters 7-9. Coffee and snacks provided! ☕📚',
        privacy: 'Department',
        likes: 15,
        comments: 12,
        shares: 6,
        liked: true,
        timestamp: '4 hours ago'
    },
    {
        id: 3,
        author: 'Alex Thompson',
        department: 'Engineering, 2023',
        avatar: 'https://images.pexels.com/photos/1040880/pexels-photo-1040880.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
        content: 'Our robotics team just won the regional competition! Thank you to everyone who supported us. Next stop: nationals! 🏆🤖',
        media: 'https://images.pexels.com/photos/2085831/pexels-photo-2085831.jpeg?auto=compress&cs=tinysrgb&w=600&h=300&fit=crop',
        privacy: 'Public',
        likes: 67,
        comments: 23,
        shares: 15,
        liked: false,
        timestamp: '1 day ago'
    }
];

let friends = {
    batch: [
        {
            id: 1,
            name: 'Jessica Park',
            info: 'Computer Science, 2025',
            avatar: 'https://images.pexels.com/photos/1065084/pexels-photo-1065084.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
            status: 'add'
        },
        {
            id: 2,
            name: 'David Kim',
            info: 'Computer Science, 2025',
            avatar: 'https://images.pexels.com/photos/1222271/pexels-photo-1222271.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
            status: 'pending'
        },
        {
            id: 3,
            name: 'Rachel Green',
            info: 'Computer Science, 2025',
            avatar: 'https://images.pexels.com/photos/1239291/pexels-photo-1239291.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
            status: 'request'
        }
    ],
    department: [
        {
            id: 4,
            name: 'Kevin Wu',
            info: 'Computer Science, 2024',
            avatar: 'https://images.pexels.com/photos/1043471/pexels-photo-1043471.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
            status: 'add'
        },
        {
            id: 5,
            name: 'Lisa Zhang',
            info: 'Computer Science, 2026',
            avatar: 'https://images.pexels.com/photos/1181686/pexels-photo-1181686.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
            status: 'add'
        },
        {
            id: 6,
            name: 'Mark Johnson',
            info: 'Computer Science, 2023',
            avatar: 'https://images.pexels.com/photos/1484794/pexels-photo-1484794.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
            status: 'pending'
        }
    ],
    interests: [
        {
            id: 7,
            name: 'Sophie Miller',
            info: 'Art Design • AI Enthusiast',
            avatar: 'https://images.pexels.com/photos/1130626/pexels-photo-1130626.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
            status: 'add'
        },
        {
            id: 8,
            name: 'Ryan Cooper',
            info: 'Business • Tech Startup',
            avatar: 'https://images.pexels.com/photos/1212984/pexels-photo-1212984.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
            status: 'request'
        },
        {
            id: 9,
            name: 'Maya Patel',
            info: 'Mathematics • ML Research',
            avatar: 'https://images.pexels.com/photos/1844012/pexels-photo-1844012.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
            status: 'add'
        }
    ]
};

let comments = {
    1: [
        {
            author: 'Anna Davis',
            avatar: 'https://images.pexels.com/photos/1181424/pexels-photo-1181424.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
            text: 'This is amazing! Can you share the code on GitHub?'
        },
        {
            author: 'Tom Wilson',
            avatar: 'https://images.pexels.com/photos/1043474/pexels-photo-1043474.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
            text: 'Great work Mike! Looking forward to your presentation.'
        },
        {
            author: 'Linda Brown',
            avatar: 'https://images.pexels.com/photos/1587009/pexels-photo-1587009.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
            text: 'Would love to collaborate on similar projects!'
        }
    ],
    2: [
        {
            author: 'James Lee',
            avatar: 'https://images.pexels.com/photos/1300402/pexels-photo-1300402.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
            text: 'Count me in! What should I bring?'
        },
        {
            author: 'Maria Garcia',
            avatar: 'https://images.pexels.com/photos/1212984/pexels-photo-1212984.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
            text: 'Perfect timing! I need help with chapter 8.'
        }
    ],
    3: [
        {
            author: 'Chris Evans',
            avatar: 'https://images.pexels.com/photos/1547971/pexels-photo-1547971.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
            text: 'Congratulations! You guys deserved this win! 🎉'
        },
        {
            author: 'Nicole Smith',
            avatar: 'https://images.pexels.com/photos/1065084/pexels-photo-1065084.jpeg?auto=compress&cs=tinysrgb&w=70&h=70&fit=crop',
            text: 'So proud of our robotics team! Good luck at nationals!'
        }
    ]
};

// Initialize the app
document.addEventListener('DOMContentLoaded', function () {
    renderPosts();
    renderFriends('batch');
    initializeEventListeners();
});

function initializeEventListeners() {
    // Filter buttons for suggested friends
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            renderFriends(this.dataset.filter);
        });
    });

    // Create post button
    document.getElementById('createPostBtn').addEventListener('click', createPost);

    // Search functionality
    document.getElementById('searchInput').addEventListener('input', handleSearch);
}

function renderPosts() {
    const container = document.getElementById('postsContainer');
    container.innerHTML = posts.map(post => `
        <div class="post-card">
            <div class="post-header">
                <img src="${post.avatar}" alt="${post.author}" class="post-avatar">
                <div class="post-user-info">
                    <h4>${post.author}</h4>
                    <p>${post.department} • ${post.timestamp}</p>
                </div>
                <div class="post-privacy">${getPrivacyIcon(post.privacy)} ${post.privacy}</div>
            </div>
            <div class="post-content">${post.content}</div>
            ${post.media ? `<div class="post-media"><img src="${post.media}" alt="Post media"></div>` : ''}
            <div class="post-stats">
                <span>${post.likes} likes</span>
                <span>${post.comments} comments • ${post.shares} shares</span>
            </div>
            <div class="post-actions">
                <button class="post-action-btn ${post.liked ? 'liked' : ''}" onclick="toggleLike(${post.id})">
                    ${post.liked ? '❤️' : '🤍'} Like
                </button>
                <button class="post-action-btn" onclick="toggleComments(${post.id})">
                    💬 Comment
                </button>
                <button class="post-action-btn">
                    📤 Share
                </button>
            </div>
            <div class="comments-section" id="comments-${post.id}">
                ${renderComments(post.id)}
            </div>
        </div>
    `).join('');
}

function renderComments(postId) {
    const postComments = comments[postId] || [];
    const visibleComments = postComments.slice(0, 2);
    const remainingCount = postComments.length - 2;

    let html = visibleComments.map(comment => `
        <div class="comment-item">
            <img src="${comment.avatar}" alt="${comment.author}" class="comment-avatar">
            <div class="comment-content">
                <div class="comment-author">${comment.author}</div>
                <div class="comment-text">${comment.text}</div>
            </div>
        </div>
    `).join('');

    if (remainingCount > 0) {
        html += `<div class="view-more-comments" onclick="showAllComments(${postId})">View ${remainingCount} more comments...</div>`;
    }

    return html;
}

function renderFriends(filter) {
    const container = document.getElementById('friendsList');
    const friendsList = friends[filter] || [];

    container.innerHTML = friendsList.map(friend => `
        <div class="friend-item">
            <img src="${friend.avatar}" alt="${friend.name}" class="friend-avatar">
            <div class="friend-info">
                <h4>${friend.name}</h4>
                <p>${friend.info}</p>
            </div>
            <div class="friend-actions">
                ${renderFriendButton(friend)}
            </div>
        </div>
    `).join('');
}

function renderFriendButton(friend) {
    switch (friend.status) {
        case 'add':
            return `<button class="friend-btn add" onclick="sendFriendRequest(${friend.id}, '${friend.name}')">Add</button>`;
        case 'pending':
            return `<button class="friend-btn pending" onclick="cancelFriendRequest(${friend.id})">Pending</button>`;
        case 'request':
            return `
                <button class="friend-btn accept" onclick="acceptFriendRequest(${friend.id}, '${friend.name}')">Accept</button>
                <button class="friend-btn reject" onclick="rejectFriendRequest(${friend.id}, '${friend.name}')">Reject</button>
            `;
        default:
            return '';
    }
}

function getPrivacyIcon(privacy) {
    switch (privacy) {
        case 'Public': return '🌍';
        case 'Department': return '🏛️';
        case 'Batch': return '👥';
        default: return '🌍';
    }
}

function toggleLike(postId) {
    const post = posts.find(p => p.id === postId);
    if (post) {
        post.liked = !post.liked;
        post.likes += post.liked ? 1 : -1;
        renderPosts();
    }
}

function toggleComments(postId) {
    const commentsSection = document.getElementById(`comments-${postId}`);
    if (commentsSection) {
        commentsSection.style.display = commentsSection.style.display === 'none' ? 'block' : 'none';
    }
}

function showAllComments(postId) {
    const postComments = comments[postId] || [];
    const container = document.getElementById(`comments-${postId}`);

    container.innerHTML = postComments.map(comment => `
        <div class="comment-item">
            <img src="${comment.avatar}" alt="${comment.author}" class="comment-avatar">
            <div class="comment-content">
                <div class="comment-author">${comment.author}</div>
                <div class="comment-text">${comment.text}</div>
            </div>
        </div>
    `).join('');
}

function createPost() {
    const textarea = document.getElementById('postText');
    const privacy = document.getElementById('privacySelect');

    if (textarea.value.trim()) {
        const newPost = {
            id: posts.length + 1,
            author: 'Sarah Johnson',
            department: 'Computer Science, 2025',
            avatar: 'https://images.pexels.com/photos/1438081/pexels-photo-1438081.jpeg?auto=compress&cs=tinysrgb&w=100&h=100&fit=crop',
            content: textarea.value,
            privacy: privacy.options[privacy.selectedIndex].text.split(' ').slice(1).join(' '),
            likes: 0,
            comments: 0,
            shares: 0,
            liked: false,
            timestamp: 'Just now'
        };

        posts.unshift(newPost);
        renderPosts();
        textarea.value = '';

        // Show success animation
        showNotification('Post created successfully!', 'success');
    }
}

function sendFriendRequest(friendId, friendName) {
    // Update friend status to pending
    updateFriendStatus(friendId, 'pending');
    showNotification(`Friend request sent to ${friendName}`, 'success');
}

function acceptFriendRequest(friendId, friendName) {
    // Remove from friends list (would be added to actual friends)
    removeFriendFromList(friendId);
    showNotification(`You are now friends with ${friendName}`, 'success');
}

function rejectFriendRequest(friendId, friendName) {
    // Remove from friends list
    removeFriendFromList(friendId);
    showNotification(`Friend request from ${friendName} rejected`, 'info');
}

function cancelFriendRequest(friendId) {
    updateFriendStatus(friendId, 'add');
    showNotification('Friend request cancelled', 'info');
}

function updateFriendStatus(friendId, newStatus) {
    // Update in all categories
    Object.keys(friends).forEach(category => {
        const friend = friends[category].find(f => f.id === friendId);
        if (friend) {
            friend.status = newStatus;
        }
    });

    // Re-render current view
    const activeFilter = document.querySelector('.filter-btn.active').dataset.filter;
    renderFriends(activeFilter);
}

function removeFriendFromList(friendId) {
    // Remove from all categories
    Object.keys(friends).forEach(category => {
        friends[category] = friends[category].filter(f => f.id !== friendId);
    });

    // Re-render current view
    const activeFilter = document.querySelector('.filter-btn.active').dataset.filter;
    renderFriends(activeFilter);
}

function handleSearch(event) {
    const searchTerm = event.target.value.toLowerCase();
    // This would typically filter posts, users, etc.
    // For demo purposes, we'll just show a visual indication
    if (searchTerm.length > 2) {
        console.log(`Searching for: ${searchTerm}`);
        // In a real app, this would trigger API calls or filter existing data
    }
}

function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 80px;
        right: 20px;
        background: ${type === 'success' ? '#4caf50' : type === 'error' ? '#f44336' : '#7c4dff'};
        color: white;
        padding: 15px 20px;
        border-radius: 8px;
        z-index: 10000;
        transform: translateX(100%);
        transition: transform 0.3s ease;
        box-shadow: 0 4px 20px rgba(0,0,0,0.1);
    `;
    notification.textContent = message;
    document.body.appendChild(notification);

    // Animate in
    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
    }, 100);

    // Remove after 3 seconds
    setTimeout(() => {
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            document.body.removeChild(notification);
        }, 300);
    }, 3000);
}

// Add smooth scrolling to top when clicking on trending items
document.addEventListener('click', function (e) {
    if (e.target.closest('.trending-item')) {
        showNotification('Feature coming soon!', 'info');
    }
});

// Add keyboard shortcuts
document.addEventListener('keydown', function (e) {
    // Ctrl/Cmd + Enter to post
    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
        const textarea = document.getElementById('postText');
        if (textarea === document.activeElement && textarea.value.trim()) {
            createPost();
        }
    }

    // Escape to clear search
    if (e.key === 'Escape') {
        const searchInput = document.getElementById('searchInput');
        if (searchInput === document.activeElement) {
            searchInput.value = '';
        }
    }
});

// Add loading states for friend actions
function addLoadingState(button) {
    const originalText = button.textContent;
    button.textContent = '...';
    button.disabled = true;

    setTimeout(() => {
        button.textContent = originalText;
        button.disabled = false;
    }, 500);
}

// Enhance friend buttons with loading states
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('friend-btn')) {
        addLoadingState(e.target);
    }
});