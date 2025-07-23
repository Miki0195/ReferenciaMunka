import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Container, Row, Col, Card, Button, Alert, Badge } from 'react-bootstrap';
import MovieService from '../services/movieService';
import { useAuth } from '../hooks/useAuth';

const MovieDetailsPage = () => {
  const [movie, setMovie] = useState(null);
  const [screenings, setScreenings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedWeek, setSelectedWeek] = useState(null);
  const [selectedDay, setSelectedDay] = useState(null);
  
  const { id } = useParams();
  const navigate = useNavigate();
  const { currentUser } = useAuth();

  useEffect(() => {
    const storedWeek = sessionStorage.getItem('selectedWeek');
    const storedDay = sessionStorage.getItem('selectedDay');
    if (storedWeek) {
      const weekNum = parseInt(storedWeek);
      setSelectedWeek(weekNum);
      
      if (storedDay) {
        const dayNum = parseInt(storedDay);
        setSelectedDay(dayNum);
      }
    }
  }, []);

  useEffect(() => {
    const fetchMovie = async () => {
      setLoading(true);
      try {
        const movieData = await MovieService.getMovieWithScreenings(id, selectedWeek, selectedDay);
        setMovie(movieData);
      
        if (movieData.screenings && Array.isArray(movieData.screenings)) {
          let filteredScreenings = [...movieData.screenings];
          
          if (selectedWeek !== null) {
            filteredScreenings = filteredScreenings.filter(screening => 
              parseInt(screening.week_number) === selectedWeek
            );
            
            if (selectedDay !== null) {
              filteredScreenings = filteredScreenings.filter(screening => 
                parseInt(screening.week_day) === selectedDay
              );
            }
          }
          
          filteredScreenings.sort((a, b) => {
            const dateA = new Date(a.date + ' ' + a.start_time);
            const dateB = new Date(b.date + ' ' + b.start_time);
            return dateA - dateB;
          });
          
          setScreenings(filteredScreenings);
        } else {
          setScreenings([]);
        }
      } catch (err) {
        setError('Failed to load movie details: ' + (err.message || String(err)));
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    fetchMovie();
  }, [id, selectedWeek, selectedDay]);

  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    
    try {
      if (typeof dateString === 'string' && dateString.includes('T')) {
        const options = { year: 'numeric', month: 'long', day: 'numeric' };
        return new Date(dateString).toLocaleDateString('hu-HU', options);
      }
      
      if (typeof dateString === 'string' && dateString.includes('-')) {
        const datePart = new Date(dateString);
        const options = { year: 'numeric', month: 'long', day: 'numeric' };
        return datePart.toLocaleDateString('hu-HU', options);
      }
      
      return new Date(dateString).toLocaleDateString('hu-HU', { year: 'numeric', month: 'long', day: 'numeric' });
    } catch (e) {
      console.error('Date formatting error:', e, dateString);
      return String(dateString);
    }
  };

  const formatTime = (timeString) => {
    if (!timeString) return 'N/A';
    
    if (typeof timeString === 'string' && timeString.includes(':')) {
      return timeString;
    }
    
    try {
      const options = { hour: '2-digit', minute: '2-digit' };
      return new Date(timeString).toLocaleTimeString('hu-HU', options);
    } catch (e) {
      console.error('Time formatting error:', e, timeString);
      return String(timeString);
    }
  };

  const handleBookScreening = (screeningId) => {
    if (!currentUser) {
      navigate('/login', { state: { from: `/movie/${id}` } });
    } else {
      navigate(`/book/${screeningId}`);
    }
  };
  
  const getDayName = (dayNumber) => {
    const days = {
      1: 'Hétfő',
      2: 'Kedd',
      3: 'Szerda',
      4: 'Csütörtök',
      5: 'Péntek',
      6: 'Szombat',
      7: 'Vasárnap'
    };
    return days[dayNumber] || '';
  };

  const getFilterInfo = () => {
    if (selectedWeek && selectedDay) {
      return `(${selectedWeek}. hét, ${getDayName(selectedDay)})`;
    } else if (selectedWeek) {
      return `(${selectedWeek}. hét)`;
    }
    return '';
  };

  const clearFilters = () => {
    setSelectedWeek(null);
    setSelectedDay(null);
    sessionStorage.removeItem('selectedWeek');
    sessionStorage.removeItem('selectedDay');
  };

  if (loading) {
    return (
      <Container className="d-flex justify-content-center align-items-center" style={{ minHeight: '50vh' }}>
        <div className="text-center">
          <div className="spinner-border text-primary mb-3" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
          <p className="text-muted">Betöltés folyamatban...</p>
        </div>
      </Container>
    );
  }

  if (error) {
    return (
      <Container className="mt-5">
        <Alert variant="danger" className="shadow-sm">
          <div className="d-flex align-items-center">
            <i className="bi bi-exclamation-triangle-fill fs-3 me-3"></i>
            <div>
              <h5 className="mb-1">Hiba történt</h5>
              <p className="mb-0">{error}</p>
            </div>
          </div>
        </Alert>
        <div className="text-center mt-4">
          <Button variant="outline-primary" onClick={() => navigate(-1)}>
            <i className="bi bi-arrow-left me-2"></i> Vissza
          </Button>
        </div>
      </Container>
    );
  }

  if (!movie) {
    return (
      <Container className="mt-5">
        <Alert variant="warning" className="shadow-sm">
          <div className="d-flex align-items-center">
            <i className="bi bi-question-circle-fill fs-3 me-3"></i>
            <div>
              <h5 className="mb-1">Film nem található</h5>
              <p className="mb-0">A keresett film nem található az adatbázisban.</p>
            </div>
          </div>
        </Alert>
        <div className="text-center mt-4">
          <Button variant="primary" onClick={() => navigate('/')}>
            <i className="bi bi-house-door me-2"></i> Vissza a főoldalra
          </Button>
        </div>
      </Container>
    );
  }

  return (
    <Container fluid="lg" className="my-5">
      <Row className="g-4">
        <Col lg={4} md={5}>
          <div className="position-lg-sticky" style={{ top: '100px' }}>
            <Card className="border-0 shadow-sm overflow-hidden">
              <Card.Img 
                src={movie.image_path} 
                alt={movie.title}
                className="img-fluid"
                style={{ maxHeight: '600px', objectFit: 'cover' }}
                onError={(e) => {
                  e.target.onerror = null;
                  e.target.src = 'https://via.placeholder.com/300x450?text=No+Image';
                }}
              />
            </Card>
            <div className="d-grid gap-2 mt-3">
              <Link to="/" className="btn btn-outline-secondary">
                <i className="bi bi-arrow-left me-2"></i> Vissza a filmekhez
              </Link>
            </div>
          </div>
        </Col>
        <Col lg={8} md={7}>
          <div className="mb-4">
            <h1 className="display-5 fw-bold mb-3">{movie.title}</h1>
            <div className="mb-4 d-flex flex-wrap gap-2">
              <Badge bg="secondary" className="px-3 py-2">{movie.genre}</Badge>
              <Badge bg="secondary" className="px-3 py-2">{movie.duration} perc</Badge>
              <Badge bg="secondary" className="px-3 py-2">{movie.release_year}</Badge>
            </div>
            <p className="lead">{movie.description}</p>
          </div>
          
          <Card className="border-0 shadow-sm mb-4">
            <Card.Body>
              <div className="d-flex flex-column flex-md-row justify-content-between align-items-start align-items-md-center mb-3 gap-2">
                <h3 className="mb-0">
                  <i className="bi bi-calendar2-week me-2"></i>
                  Vetítések {getFilterInfo()}
                </h3>
                {selectedWeek && (
                  <Button 
                    variant="outline-primary" 
                    size="sm" 
                    onClick={clearFilters}
                  >
                    <i className="bi bi-funnel me-1"></i> Szűrők törlése
                  </Button>
                )}
              </div>
              
              {screenings.length === 0 ? (
                <Alert variant="info" className="border-0 shadow-sm">
                  <div className="d-flex align-items-center">
                    <i className="bi bi-info-circle-fill me-3 fs-4"></i>
                    <div>
                      {selectedWeek ? 
                        `Nincs meghirdetett vetítés a kiválasztott időszakban` : 
                        `Jelenleg nincs meghirdetett vetítés ehhez a filmhez`}
                    </div>
                  </div>
                </Alert>
              ) : (
                <div className="screening-cards">
                  <Row xs={1} sm={1} md={2} lg={2} className="g-3">
                    {screenings.map((screening) => (
                      <Col key={screening.id}>
                        <Card className="h-100 border-0 shadow-sm screening-card">
                          <Card.Body>
                            <div className="d-flex flex-column flex-sm-row justify-content-between align-items-start mb-3 gap-2">
                              <div>
                                <h5 className="mb-1">
                                  {screening.date ? formatDate(screening.date) : formatDate(screening.start_time)}
                                </h5>
                                <h6 className="text-muted mb-0">
                                  <i className="bi bi-clock me-1"></i>
                                  {screening.start_time ? formatTime(screening.start_time) : ''}
                                </h6>
                              </div>
                              <Badge bg="primary" className="px-2 py-1 align-self-start">
                                {getDayName(screening.week_day)}
                              </Badge>
                            </div>
                            <div className="d-grid">
                              <Button 
                                variant="primary" 
                                onClick={() => handleBookScreening(screening.id)}
                                className="d-flex align-items-center justify-content-center"
                              >
                                <i className="bi bi-ticket-perforated me-2"></i> Jegyfoglalás
                              </Button>
                            </div>
                          </Card.Body>
                        </Card>
                      </Col>
                    ))}
                  </Row>
                </div>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default MovieDetailsPage; 