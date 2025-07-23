import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { Container, Nav, Navbar as BootstrapNavbar, Button } from 'react-bootstrap';

const Navbar = () => {
  const { currentUser, logout, isAdmin } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <BootstrapNavbar bg="dark" variant="dark" expand="lg" fixed="top" className="shadow w-100">
      <Container fluid="lg">
        <BootstrapNavbar.Brand as={Link} to="/">
          <span className="text-primary fw-bold">t</span>
          <span className="fw-bold">IKera</span>
        </BootstrapNavbar.Brand>
        <BootstrapNavbar.Toggle aria-controls="basic-navbar-nav" />
        <BootstrapNavbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
            <Nav.Link as={Link} to="/" className="mx-1">
              <i className="bi bi-film me-1"></i> Moziműsor
            </Nav.Link>
            
            {currentUser ? (
              <>
                <Nav.Link as={Link} to="/bookings" className="mx-1">
                  <i className="bi bi-ticket-perforated me-1"></i> Foglalásaim
                </Nav.Link>
                
                {isAdmin() && (
                  <>
                    <Nav.Link as={Link} to="/add-movie" className="mx-1">
                      <i className="bi bi-plus-circle me-1"></i> Film hozzáadása
                    </Nav.Link>
                    <Nav.Link as={Link} to="/add-screening" className="mx-1">
                      <i className="bi bi-calendar-plus me-1"></i> Vetítés hozzáadása
                    </Nav.Link>
                  </>
                )}
              </>
            ) : (
              <>
                <Nav.Link as={Link} to="/login" className="mx-1">
                  <i className="bi bi-box-arrow-in-right me-1"></i> Bejelentkezés
                </Nav.Link>
                <Nav.Link as={Link} to="/register" className="mx-1">
                  <i className="bi bi-person-plus me-1"></i> Regisztráció
                </Nav.Link>
              </>
            )}
          </Nav>
          
          {currentUser && (
            <Nav className="align-items-center">
              <Nav.Item className="text-light d-flex align-items-center me-3">
                <i className="bi bi-person-circle me-1"></i>
                {currentUser.name}
              </Nav.Item>
              <Button variant="outline-light" size="sm" onClick={handleLogout}>
                <i className="bi bi-box-arrow-right me-1"></i> Kijelentkezés
              </Button>
            </Nav>
          )}
        </BootstrapNavbar.Collapse>
      </Container>
    </BootstrapNavbar>
  );
};

export default Navbar; 