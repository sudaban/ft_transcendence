import { browser } from '$app/environment';

export interface AuthUser
{
  id: string;
  username: string;
  email: string;
  avatar?: string;
  role?: string;
}

function parseJwt(token: string): any
{
  try
  {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(atob(base64).split('').map(function (c)
    {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(jsonPayload);
  }
  catch (e)
  {
    return null;
  }
}

class AuthStore
{
  isAuthenticated = $state(false);
  token = $state<string | null>(null);
  user = $state<AuthUser | null>(null);

  init()
  {
    if (!browser)
      return;
    const storedToken = localStorage.getItem('token');

    if (storedToken)
    {
      const decoded = parseJwt(storedToken);
      if (decoded && decoded.exp * 1000 > Date.now())
      {
        this.token = storedToken;
        this.isAuthenticated = true;

        const nameIdKey = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
        const nameKey = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
        const emailKey = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
        const roleKey = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

        const extractedUsername = decoded[nameKey] || decoded.unique_name || decoded.name || 'User';

        const storedAvatar = localStorage.getItem('avatar');

        this.user = {
          id: decoded[nameIdKey] || decoded.sub || '',
          username: extractedUsername,
          email: decoded[emailKey] || decoded.email || '',
          avatar: storedAvatar || '',
          role: decoded[roleKey] || decoded.role || 'User'
        };
      }
      else
      {
        this.logout(false);
      }
    }
  }

  login(newToken: string)
  {
    if (!browser)
      return;
    localStorage.setItem('token', newToken);
    this.init();
  }

  logout(redirect: boolean = true)
  {
    if (!browser)
      return;
    localStorage.removeItem('token');
    localStorage.removeItem('avatar');
    this.token = null;
    this.isAuthenticated = false;
    this.user = null;
    if (redirect)
    {
      window.location.href = '/login';
    }
  }
}

export const authStore = new AuthStore();
