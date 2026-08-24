(function () {
    const toggle = document.getElementById('chatToggle');
    const panel = document.getElementById('chatPanel');
    const closeBtn = document.getElementById('chatClose');
    const body = document.getElementById('chatBody');
    const form = document.getElementById('chatForm');
    const input = document.getElementById('chatInput');

    if (!toggle || !panel) return;

    function sessionId() {
        let id = localStorage.getItem('pt_chat_session');
        if (!id) {
            id = 'sess-' + Math.random().toString(36).slice(2) + Date.now().toString(36);
            localStorage.setItem('pt_chat_session', id);
        }
        return id;
    }

    function addMessage(text, sender) {
        const el = document.createElement('div');
        el.className = 'chat-msg ' + (sender === 'User' ? 'user' : 'bot');
        if (sender === 'User') {
            el.textContent = text;
        } else {
            // Escape HTML characters to prevent XSS
            let formatted = text
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/"/g, '&quot;')
                .replace(/'/g, '&#039;');
            // Parse Markdown Bold: **text** -> <strong>text</strong>
            formatted = formatted.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
            // Parse Markdown Links: [text](url) -> <a href="url">text</a>
            formatted = formatted.replace(/\[(.*?)\]\((.*?)\)/g, '<a href="$2" style="text-decoration: underline; font-weight: bold; color: inherit;">$1</a>');
            // Replace newlines with line breaks
            formatted = formatted.replace(/\n/g, '<br />');
            el.innerHTML = formatted;
        }
        body.appendChild(el);
        body.scrollTop = body.scrollHeight;
    }

    toggle.addEventListener('click', () => panel.classList.add('open'));
    closeBtn.addEventListener('click', () => panel.classList.remove('open'));

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const text = input.value.trim();
        if (!text) return;
        addMessage(text, 'User');
        input.value = '';

        try {
            const res = await fetch('/Chat/Send', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ sessionId: sessionId(), messageText: text })
            });
            if (res.ok) {
                const data = await res.json();
                const last = data.messages[data.messages.length - 1];
                if (last) addMessage(last.messageText, 'Bot');
            } else {
                addMessage("Sorry, I couldn't reach support right now. Please try again shortly.", 'Bot');
            }
        } catch {
            addMessage("Sorry, I couldn't reach support right now. Please try again shortly.", 'Bot');
        }
    });
})();
