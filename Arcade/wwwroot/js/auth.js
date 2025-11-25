window.arcadeAuth = {
    login: async function (dto) {
        try {
            const resp = await fetch('api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(dto),
                credentials: 'include'
            });

            let payload = null;
            try {
                payload = await resp.json();
            } catch (e) {
            }

            return {
                ok: resp.ok,
                status: resp.status,
                message: payload && payload.message ? payload.message : null
            };
        } catch (e) {
            return {
                ok: false,
                status: 0,
                message: e.message || 'Netzwerkfehler'
            };
        }
    },

    logout: async function () {
        try {
            await fetch('api/auth/logout', {
                method: 'POST',
                credentials: 'include'
            });
        } catch (e) {
      
        }
    }
};
