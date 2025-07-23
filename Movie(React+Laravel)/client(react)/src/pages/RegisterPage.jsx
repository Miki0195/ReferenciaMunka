import { useState } from 'react';
import { Form, Button, Container, Card, Alert } from 'react-bootstrap';
import { useAuth } from '../hooks/useAuth';
import { useNavigate, Link } from 'react-router-dom';

const RegisterPage = () => {
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    password: '',
    password_confirmation: '',
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  
  const { register } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (formData.password !== formData.password_confirmation) {
      return setError('A jelszavak nem egyeznek');
    }
    
    setError('');
    setLoading(true);

    try {
      await register(formData);
      navigate('/');
    } catch (err) {
      setError(err.message || 'Failed to create an account');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: '70vh' }}>
      <Card style={{ width: '400px' }}>
        <Card.Body>
          <h2 className="text-center mb-4">Regisztráció</h2>
          
          {error && <Alert variant="danger">{error}</Alert>}
          
          <Form onSubmit={handleSubmit}>
            <Form.Group className="mb-3" controlId="name">
              <Form.Label>Név</Form.Label>
              <Form.Control 
                type="text" 
                name="name"
                value={formData.name}
                onChange={handleChange}
                required 
              />
            </Form.Group>
            
            <Form.Group className="mb-3" controlId="email">
              <Form.Label>Email cím</Form.Label>
              <Form.Control 
                type="email" 
                name="email"
                value={formData.email}
                onChange={handleChange}
                required 
              />
            </Form.Group>

            <Form.Group className="mb-3" controlId="password">
              <Form.Label>Jelszó</Form.Label>
              <Form.Control 
                type="password" 
                name="password"
                value={formData.password}
                onChange={handleChange}
                required 
              />
            </Form.Group>

            <Form.Group className="mb-3" controlId="password_confirmation">
              <Form.Label>Jelszó megerősítése</Form.Label>
              <Form.Control 
                type="password" 
                name="password_confirmation"
                value={formData.password_confirmation}
                onChange={handleChange}
                required 
              />
            </Form.Group>

            <Button 
              className="w-100 mt-3" 
              variant="primary" 
              type="submit"
              disabled={loading}
            >
              {loading ? 'Regisztráció...' : 'Regisztráció'}
            </Button>
          </Form>

          <div className="text-center mt-3">
            Már van fiókod? <Link to="/login">Bejelentkezés</Link>
          </div>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default RegisterPage; 