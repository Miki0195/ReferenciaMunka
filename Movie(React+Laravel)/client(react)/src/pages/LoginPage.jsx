import { useState } from 'react';
import { Form, Button, Container, Card, Alert, Badge } from 'react-bootstrap';
import { useAuth } from '../hooks/useAuth';
import { useNavigate, Link } from 'react-router-dom';

const LoginPage = () => {
  const [formData, setFormData] = useState()
  const [error, setError] = useState('');
  const [debugInfo, setDebugInfo] = useState('');
  const [loading, setLoading] = useState(false);
  
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setDebugInfo('');
    setLoading(true);

    try {
      setDebugInfo('Attempting login with: ' + JSON.stringify(formData));
      
      const result = await login(formData);
      setDebugInfo('Login successful. User: ' + JSON.stringify(result.user));
      
      navigate('/');
    } catch (err) {
      setError(err.message || 'Failed to sign in');
      setDebugInfo('Login error: ' + JSON.stringify(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container className="d-flex justify-content-center align-items-center py-5" style={{ minHeight: '80vh' }}>
      <Card className="border-0 shadow-sm" style={{ width: '450px' }}>
        <Card.Body className="p-4">
          <div className="text-center mb-4">
            <h2 className="fw-bold mb-2">Bejelentkezés</h2>
            <p className="text-muted">Jelentkezz be a foglalások kezeléséhez</p>
          </div>
          
          {error && (
            <Alert variant="danger" className="d-flex align-items-center">
              <i className="bi bi-exclamation-triangle-fill me-2 fs-5"></i>
              <div>{error}</div>
            </Alert>
          )}
          
          <Form onSubmit={handleSubmit}>
            <Form.Group className="mb-3" controlId="email">
              <Form.Label>
                <i className="bi bi-envelope me-2"></i>
                Email cím
              </Form.Label>
              <Form.Control 
                type="email" 
                name="email"
                onChange={handleChange}
                required 
                className="py-2"
                placeholder="Add meg az email címed"
              />
            </Form.Group>

            <Form.Group className="mb-4" controlId="password">
              <Form.Label>
                <i className="bi bi-lock me-2"></i>
                Jelszó
              </Form.Label>
              <Form.Control 
                type="password" 
                name="password"
                onChange={handleChange}
                required 
                className="py-2"
                placeholder="Add meg a jelszavad"
              />
            </Form.Group>

            <div className="d-grid">
              <Button 
                variant="primary" 
                type="submit"
                disabled={loading}
                className="py-2"
              >
                {loading ? (
                  <>
                    <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                    Bejelentkezés...
                  </>
                ) : (
                  <>
                    <i className="bi bi-box-arrow-in-right me-2"></i>
                    Bejelentkezés
                  </>
                )}
              </Button>
            </div>
          </Form>

          <div className="text-center mt-4 py-3 border-top">
            <p className="mb-0">
              Nincs még fiókod? 
              <Link to="/register" className="ms-2 fw-bold text-decoration-none">
                Regisztráció
              </Link>
            </p>
          </div>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default LoginPage; 