import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Box,
  TextField,
  Button,
  Typography,
  Alert,
  CircularProgress,
  Paper,
} from '@mui/material';
import { authService } from '../services/authService';

export const LoginPage = () => {
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await authService.login({
        username,
        password,
      });

      // Store the tokens and user info
      if (response.accessToken) {
        localStorage.setItem('authToken', response.accessToken);
        if (response.refreshToken) {
          localStorage.setItem('refreshToken', response.refreshToken);
        }
        if (response.user) {
          localStorage.setItem('user', JSON.stringify(response.user));
        }
        navigate('/');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Login failed. Please try again.');
      console.error('Login error:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          minHeight: '100vh',
          py: 4,
        }}
      >
        <Paper elevation={3} sx={{ p: 4 }}>
          <Typography
            variant="h4"
            component="h1"
            sx={{
              mb: 3,
              textAlign: 'center',
              fontWeight: 'bold',
              color: 'primary.main',
            }}
          >
            School Management System
          </Typography>

          <Typography
            variant="h6"
            component="h2"
            sx={{
              mb: 3,
              textAlign: 'center',
              color: 'text.secondary',
            }}
          >
            Login
          </Typography>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleLogin} noValidate>
            <TextField
              margin="normal"
              required
              fullWidth
              id="username"
              label="Username"
              name="username"
              autoComplete="username"
              autoFocus
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              disabled={loading}
            />
            <TextField
              margin="normal"
              required
              fullWidth
              name="password"
              label="Password"
              type="password"
              id="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              disabled={loading}
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
              disabled={loading}
            >
              {loading ? <CircularProgress size={24} /> : 'Sign In'}
            </Button>
          </Box>

          <Typography
            variant="body2"
            sx={{
              mt: 2,
              textAlign: 'center',
              color: 'text.secondary',
            }}
          >
            Demo credentials: admin / password
          </Typography>
        </Paper>
      </Box>
    </Container>
  );
};
