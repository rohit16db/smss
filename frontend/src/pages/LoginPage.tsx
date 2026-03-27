import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  Container,
  Box,
  TextField,
  Button,
  Typography,
  Alert,
  CircularProgress,
  Paper,
  InputAdornment,
  IconButton,
  Divider,
  Fade,
  Zoom,
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  PersonOutline,
  LockOutlined,
  SchoolOutlined,
} from '@mui/icons-material';
import { authService } from '../services/authService';

export const LoginPage = () => {
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

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
    <Box
      sx={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        position: 'relative',
        overflow: 'hidden',
        '&::before': {
          content: '""',
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'radial-gradient(circle at 20% 50%, rgba(255,255,255,0.1) 0%, transparent 50%)',
          pointerEvents: 'none',
        },
        '&::after': {
          content: '""',
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'radial-gradient(circle at 80% 80%, rgba(255,255,255,0.1) 0%, transparent 50%)',
          pointerEvents: 'none',
        },
      }}
    >
      <Container maxWidth="lg">
        <Zoom in timeout={800}>
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: { xs: 0, md: 6 },
              flexWrap: { xs: 'wrap', md: 'nowrap' },
            }}
          >
            {/* Left Side - Illustration/Info */}
            <Fade in timeout={1000}>
              <Box
                sx={{
                  flex: 1,
                  display: { xs: 'none', md: 'block' },
                  color: 'white',
                  pr: 4,
                }}
              >
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    mb: 4,
                    gap: 2,
                  }}
                >
                  <SchoolOutlined sx={{ fontSize: 60 }} />
                  <Typography variant="h3" fontWeight="bold">
                    SMS
                  </Typography>
                </Box>
                <Typography variant="h4" fontWeight="bold" mb={2}>
                  Welcome Back!
                </Typography>
                <Typography variant="h6" sx={{ opacity: 0.9, mb: 4 }}>
                  School Management System
                </Typography>
                <Typography variant="body1" sx={{ opacity: 0.8, lineHeight: 1.8 }}>
                  Manage your school operations efficiently with our comprehensive platform.
                  Track students, Staffs, attendance, fees, and much more in one place.
                </Typography>
                <Box sx={{ mt: 4, display: 'flex', gap: 3 }}>
                  <Box>
                    <Typography variant="h4" fontWeight="bold">500+</Typography>
                    <Typography variant="body2" sx={{ opacity: 0.8 }}>Students</Typography>
                  </Box>
                  <Box>
                    <Typography variant="h4" fontWeight="bold">50+</Typography>
                    <Typography variant="body2" sx={{ opacity: 0.8 }}>Staffs</Typography>
                  </Box>
                  <Box>
                    <Typography variant="h4" fontWeight="bold">20+</Typography>
                    <Typography variant="body2" sx={{ opacity: 0.8 }}>Classes</Typography>
                  </Box>
                </Box>
              </Box>
            </Fade>

            {/* Right Side - Login Form */}
            <Fade in timeout={1200}>
              <Paper
                elevation={24}
                sx={{
                  p: { xs: 3, sm: 5 },
                  width: { xs: '100%', sm: 450 },
                  maxWidth: 450,
                  borderRadius: 4,
                  background: 'rgba(255, 255, 255, 0.98)',
                  backdropFilter: 'blur(10px)',
                }}
              >
                {/* Logo/Icon */}
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'center',
                    mb: 3,
                  }}
                >
                  <Box
                    sx={{
                      width: 80,
                      height: 80,
                      borderRadius: '50%',
                      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      boxShadow: '0 8px 16px rgba(102, 126, 234, 0.4)',
                    }}
                  >
                    <SchoolOutlined sx={{ fontSize: 40, color: 'white' }} />
                  </Box>
                </Box>

                <Typography
                  variant="h4"
                  component="h1"
                  sx={{
                    mb: 1,
                    textAlign: 'center',
                    fontWeight: 'bold',
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    WebkitBackgroundClip: 'text',
                    WebkitTextFillColor: 'transparent',
                  }}
                >
                  Sign In
                </Typography>

                <Typography
                  variant="body2"
                  sx={{
                    mb: 4,
                    textAlign: 'center',
                    color: 'text.secondary',
                  }}
                >
                  Enter your credentials to access your account
                </Typography>

                {error && (
                  <Fade in>
                    <Alert 
                      severity="error" 
                      sx={{ 
                        mb: 3,
                        borderRadius: 2,
                      }}
                    >
                      {error}
                    </Alert>
                  </Fade>
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
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <PersonOutline sx={{ color: 'primary.main' }} />
                        </InputAdornment>
                      ),
                    }}
                    sx={{
                      '& .MuiOutlinedInput-root': {
                        borderRadius: 2,
                        boxShadow: '0 0 0 rgba(102, 126, 234, 0)',
                        transition: 'box-shadow 0.3s ease',
                        '&:hover': {
                          boxShadow: '0 4px 12px rgba(102, 126, 234, 0.15)',
                        },
                        '&.Mui-focused': {
                          boxShadow: '0 4px 12px rgba(102, 126, 234, 0.25)',
                        },
                      },
                    }}
                  />
                  <TextField
                    margin="normal"
                    required
                    fullWidth
                    name="password"
                    label="Password"
                    type={showPassword ? 'text' : 'password'}
                    id="password"
                    autoComplete="current-password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    disabled={loading}
                    InputProps={{
                      startAdornment: (
                        <InputAdornment position="start">
                          <LockOutlined sx={{ color: 'primary.main' }} />
                        </InputAdornment>
                      ),
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton
                            aria-label="toggle password visibility"
                            onClick={() => setShowPassword(!showPassword)}
                            edge="end"
                          >
                            {showPassword ? <VisibilityOff /> : <Visibility />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    }}
                    sx={{
                      '& .MuiOutlinedInput-root': {
                        borderRadius: 2,
                        boxShadow: '0 0 0 rgba(102, 126, 234, 0)',
                        transition: 'box-shadow 0.3s ease',
                        '&:hover': {
                          boxShadow: '0 4px 12px rgba(102, 126, 234, 0.15)',
                        },
                        '&.Mui-focused': {
                          boxShadow: '0 4px 12px rgba(102, 126, 234, 0.25)',
                        },
                      },
                    }}
                  />
                  <Button
                    type="submit"
                    fullWidth
                    variant="contained"
                    sx={{
                      mt: 4,
                      mb: 2,
                      py: 1.5,
                      borderRadius: 2,
                      fontSize: '1rem',
                      fontWeight: 'bold',
                      textTransform: 'none',
                      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                      boxShadow: '0 4px 15px rgba(102, 126, 234, 0.4)',
                      transition: 'all 0.3s ease',
                      '&:hover': {
                        background: 'linear-gradient(135deg, #764ba2 0%, #667eea 100%)',
                        boxShadow: '0 6px 20px rgba(102, 126, 234, 0.6)',
                        transform: 'translateY(-2px)',
                      },
                      '&:disabled': {
                        background: 'linear-gradient(135deg, #ccc 0%, #999 100%)',
                      },
                    }}
                    disabled={loading}
                  >
                    {loading ? (
                      <CircularProgress size={24} sx={{ color: 'white' }} />
                    ) : (
                      'Sign In'
                    )}
                  </Button>

                  <Box sx={{ textAlign: 'center', mb: 3 }}>
                    <Link to="/forgot-password" style={{ textDecoration: 'none' }}>
                      <Typography
                        variant="body2"
                        sx={{
                          color: 'primary.main',
                          fontWeight: 500,
                          transition: 'all 0.2s ease',
                          '&:hover': {
                            color: 'secondary.main',
                            textDecoration: 'underline',
                          },
                        }}
                      >
                        Forgot Password?
                      </Typography>
                    </Link>
                  </Box>

                  <Divider sx={{ my: 3 }}>
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                      Demo Access
                    </Typography>
                  </Divider>

                  <Box
                    sx={{
                      textAlign: 'center',
                      p: 2,
                      borderRadius: 2,
                      bgcolor: 'grey.50',
                      border: '1px dashed',
                      borderColor: 'grey.300',
                    }}
                  >
                    <Typography
                      variant="body2"
                      sx={{
                        color: 'text.secondary',
                        fontFamily: 'monospace',
                      }}
                    >
                      <strong>Username:</strong> admin
                      <br />
                      <strong>Password:</strong> Admin@123
                    </Typography>
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
                  © 2026 School Management System. All rights reserved.
                </Typography>
              </Paper>
            </Fade>
          </Box>
        </Zoom>
      </Container>
    </Box>
  );
};
