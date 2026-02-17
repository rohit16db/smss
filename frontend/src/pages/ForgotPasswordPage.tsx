import { useState } from 'react';
import { Link } from 'react-router-dom';
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

export const ForgotPasswordPage = () => {
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      await authService.forgotPassword(email);
      setSuccess('If an account exists with this email, a password reset link has been sent. Please check your email.');
    } catch (err: unknown) {
      setError('An error occurred. Please try again.');
      console.error('Forgot password error:', err);
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
            variant="h5"
            component="h1"
            sx={{
              mb: 3,
              textAlign: 'center',
              fontWeight: 'bold',
              color: 'primary.main',
            }}
          >
            Forgot Password
          </Typography>

          <Typography
            variant="body2"
            sx={{
              mb: 3,
              textAlign: 'center',
              color: 'text.secondary',
            }}
          >
            Enter your email address and we'll send you a link to reset your password.
          </Typography>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          {success && (
            <Alert severity="success" sx={{ mb: 2 }}>
              {success}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit} noValidate>
            <TextField
              margin="normal"
              required
              fullWidth
              id="email"
              label="Email Address"
              name="email"
              autoComplete="email"
              autoFocus
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={loading}
            />
            <Button
              type="submit"
              fullWidth
              variant="contained"
              sx={{ mt: 3, mb: 2 }}
              disabled={loading}
            >
              {loading ? <CircularProgress size={24} /> : 'Send Reset Link'}
            </Button>
            <Box sx={{ textAlign: 'center' }}>
              <Link to="/login" style={{ textDecoration: 'none' }}>
                <Typography variant="body2" color="primary">
                  Back to Login
                </Typography>
              </Link>
            </Box>
          </Box>

          <Typography
            variant="caption"
            sx={{
              mt: 3,
              display: 'block',
              textAlign: 'center',
              color: 'text.secondary',
            }}
          >
            Note: In development mode, the reset token will be stored in the database.
            Check the database for the PasswordResetToken field.
          </Typography>
        </Paper>
      </Box>
    </Container>
  );
};
