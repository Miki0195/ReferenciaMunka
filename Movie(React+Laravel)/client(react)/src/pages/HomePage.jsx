import { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Button, Alert, Form, Nav, Badge } from 'react-bootstrap';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import MovieService from '../services/movieService';
import ScreeningService from '../services/screeningService';
import { useAuth } from '../hooks/useAuth';

const HomePage = () => {
  const [movies, setMovies] = useState([]);
  const [screenings, setScreenings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [selectedWeek, setSelectedWeek] = useState(null);
  const [selectedDay, setSelectedDay] = useState(null);
  const [weeks, setWeeks] = useState([]);
  const [debugInfo, setDebugInfo] = useState('');
  
  const { currentUser, isAdmin } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    if (location.state?.message) {
      setSuccessMessage(location.state.message);
      navigate(location.pathname, { replace: true, state: {} });
      
      setTimeout(() => {
        setSuccessMessage('');
      }, 5000);
    }
  }, [location.state, location.pathname, navigate]);

  useEffect(() => {
    const now = new Date();
    const currentWeek = getWeekNumber(now);
    
    const nextWeeks = [];
    for (let i = 0; i < 7; i++) {
      const weekDate = new Date(now);
      weekDate.setDate(now.getDate() + (i * 7));
      const weekNum = getWeekNumber(weekDate);
      nextWeeks.push({
        number: weekNum,
        label: `Hét ${weekNum}`
      });
    }
    
    setWeeks(nextWeeks);
    setSelectedWeek(currentWeek);
  }, []);

  const getWeekNumber = (date) => {
    const d = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
    const dayNum = d.getUTCDay() || 7;
    d.setUTCDate(d.getUTCDate() + 4 - dayNum);
    const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
    return Math.ceil((((d - yearStart) / 86400000) + 1) / 7);
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

  const fetchData = async () => {
    setLoading(true);
    setDebugInfo('');
    setError('');
    try {
      const screeningsData = await ScreeningService.getAllScreenings();
      if (Array.isArray(screeningsData)) {
        setScreenings(screeningsData);
        setDebugInfo(prev => prev + `Loaded ${screeningsData.length} screenings. `);
      } else {
        setScreenings([]);
        setDebugInfo(prev => prev + 'Warning: Screenings data is not an array. ');
      }
      
      let moviesData;
      
      if (selectedWeek !== null) {
        setDebugInfo(prev => prev + `Fetching movies for week ${selectedWeek}${selectedDay ? `, day ${selectedDay}` : ''}. `);
        moviesData = await MovieService.getMoviesByWeek(selectedWeek, selectedDay);
      } else {
        setDebugInfo(prev => prev + 'Fetching all movies. ');
        moviesData = await MovieService.getAllMovies();
      }
      
      if (Array.isArray(moviesData)) {
        setMovies(moviesData);
        setDebugInfo(prev => prev + `Loaded ${moviesData.length} movies.`);
      } else {
        setMovies([]);
        setDebugInfo(prev => prev + 'Warning: Movies data is not an array.');
      }
    } catch (err) {
      setError('Failed to fetch data: ' + (err.message || String(err)));
      setDebugInfo(prev => prev + `Error: ${err.message || String(err)}`);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [selectedWeek, selectedDay]);

  useEffect(() => {
    if (successMessage) {
      fetchData();
    }
  }, [successMessage]);

  const handleRefresh = () => {
    fetchData();
  };

  const handleWeekSelect = (weekNumber) => {
    setSelectedWeek(weekNumber);
    setSelectedDay(null);
  };

  const handleDaySelect = (dayNumber) => {
    setSelectedDay(dayNumber === selectedDay ? null : dayNumber);
  };

  const handleMovieClick = (id) => {
    if (selectedWeek) {
      sessionStorage.setItem('selectedWeek', selectedWeek);
      if (selectedDay) {
        sessionStorage.setItem('selectedDay', selectedDay);
      } else {
        sessionStorage.removeItem('selectedDay');
      }
    } else {
      sessionStorage.removeItem('selectedWeek');
      sessionStorage.removeItem('selectedDay');
    }
    
    navigate(`/movie/${id}`);
  };

  const handleDeleteMovie = async (id, e) => {
    e.stopPropagation();
    
    if (window.confirm('Biztosan törölni szeretnéd ezt a filmet?')) {
      try {
        await MovieService.deleteMovie(id);
        await fetchData();
        setSuccessMessage('Film sikeresen törölve!');
      } catch (err) {
        setError('Failed to delete movie');
        console.error(err);
      }
    }
  };

  const handleDeleteScreening = async (id, e) => {
    e.stopPropagation();
    
    if (window.confirm('Biztosan törölni szeretnéd ezt a vetítést?')) {
      try {
        await ScreeningService.deleteScreening(id);
        await fetchData();
        setSuccessMessage('Vetítés sikeresen törölve!');
      } catch (err) {
        setError('Failed to delete screening');
        console.error(err);
      }
    }
  };

  if (loading) {
    return <Container className="text-center mt-5"><div>Loading...</div></Container>;
  }

  const getScreeningsForMovie = (movie) => {
    if (movie.screenings && Array.isArray(movie.screenings)) {
      return movie.screenings;
    }
    
    const movieScreenings = screenings.filter(screening => screening.movie_id === movie.id);
    
    if (!selectedWeek) {
      return movieScreenings;
    }
    
    return movieScreenings.filter(screening => {
      const weekMatch = parseInt(screening.week_number) === parseInt(selectedWeek);
      const dayMatch = selectedDay ? parseInt(screening.week_day) === parseInt(selectedDay) : true;
      return weekMatch && dayMatch;
    });
  };

  const formatDate = (dateString, timeString) => {
    if (!dateString) return 'N/A';
    
    try {
      if (typeof dateString === 'string' && dateString.includes('T')) {
        const options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
        return new Date(dateString).toLocaleDateString('hu-HU', options);
      }
      
      if (typeof dateString === 'string' && dateString.includes('-')) {
        const datePart = new Date(dateString);
        const options = { year: 'numeric', month: 'long', day: 'numeric' };
        
        if (timeString) {
          return `${datePart.toLocaleDateString('hu-HU', options)}, ${timeString}`;
        }
        
        return datePart.toLocaleDateString('hu-HU', options);
      }
      
      return new Date(dateString).toLocaleDateString('hu-HU', { 
        year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' 
      });
    } catch (e) {
      console.error('Date formatting error:', e, dateString);
      return String(dateString); 
    }
  };

  return (
    <Container fluid="lg">
      <div className="text-center mb-5">
        <h1 className="display-4 fw-bold mb-3">Moziműsor</h1>
        <p className="lead text-muted">Fedezd fel a legjobb filmeket és foglalj jegyet azonnal</p>
      </div>
      
      {!currentUser && (
        <Alert variant="info" className="mb-4 shadow-sm border-0">
          <div className="d-flex align-items-center">
            <i className="bi bi-info-circle-fill me-2 fs-4"></i>
            <div>
              A jegyfoglaláshoz kérjük <Link to="/login" className="fw-bold">jelentkezz be</Link> vagy <Link to="/register" className="fw-bold">regisztrálj</Link>.
            </div>
          </div>
        </Alert>
      )}
      
      {error && <Alert variant="danger">{error}</Alert>}
      {successMessage && (
        <Alert variant="success" className="d-flex justify-content-between align-items-center">
          <div>
            <i className="bi bi-check-circle me-2"></i>
            {successMessage}
          </div>
          <Button 
            variant="outline-success" 
            size="sm"
            onClick={() => setSuccessMessage('')}
          >
            <i className="bi bi-x"></i>
          </Button>
        </Alert>
      )}
      
      {/* Week and day selector card */}
      <Card className="mb-4 shadow-sm border-0">
        <Card.Body>
          <div className="mb-3">
            <h5 className="mb-3">
              <i className="bi bi-calendar-week me-2"></i>
              Válassz hetet:
            </h5>
            <Nav variant="tabs" className="mb-3">
              {weeks.map((week) => (
                <Nav.Item key={week.number}>
                  <Nav.Link 
                    active={selectedWeek === week.number}
                    onClick={() => handleWeekSelect(week.number)}
                    className="px-3 py-2"
                  >
                    {week.label}
                  </Nav.Link>
                </Nav.Item>
              ))}
            </Nav>
          </div>
          
          {/* Day selector - only show if a week is selected */}
          {selectedWeek && (
            <div>
              <h5 className="mb-3">
                <i className="bi bi-calendar-day me-2"></i>
                Válassz napot:
              </h5>
              <Nav variant="pills" className="flex-wrap">
                {[1, 2, 3, 4, 5, 6, 7].map((day) => (
                  <Nav.Item key={day} className="mb-2">
                    <Nav.Link
                      active={selectedDay === day}
                      onClick={() => handleDaySelect(day)}
                      className="px-3 py-2"
                    >
                      {getDayName(day)}
                    </Nav.Link>
                  </Nav.Item>
                ))}
              </Nav>
            </div>
          )}
        </Card.Body>
      </Card>
      
      {movies.length === 0 ? (
        <Alert variant="info" className="text-center p-5 shadow-sm border-0">
          <i className="bi bi-film x-5 d-block mb-3" style={{ fontSize: '3rem' }}></i>
          <h4>Nincsenek filmek az adatbázisban a kiválasztott időszakban.</h4>
          <p className="mb-0 text-muted">Kérjük, válassz másik hetet vagy napot.</p>
        </Alert>
      ) : (
        <div className="movie-grid">
          {movies.map(movie => {
            const movieScreenings = getScreeningsForMovie(movie);
            
            return (
              <Card 
                key={movie.id}
                className="movie-card h-100" 
                onClick={() => handleMovieClick(movie.id)}
                style={{ cursor: 'pointer' }}
              >
                <div className="position-relative">
                  <Card.Img 
                    variant="top" 
                    src={movie.image_path} 
                    alt={movie.title}
                    onError={(e) => {
                      e.target.onerror = null;
                      e.target.src = 'https://via.placeholder.com/300x450?text=No+Image';
                    }}
                  />
                  <div className="position-absolute top-0 end-0 m-2">
                    <Badge bg="dark" className="opacity-75 p-2">
                      {movie.duration} perc
                    </Badge>
                  </div>
                </div>
                <Card.Body>
                  <Card.Title className="fs-5 mb-2">{movie.title}</Card.Title>
                  <div className="d-flex mb-3">
                    <Badge bg="secondary" className="me-2">{movie.genre}</Badge>
                    <Badge bg="secondary">{movie.release_year}</Badge>
                  </div>
                  <Card.Text className="small text-muted mb-3 movie-description">
                    {movie.description.length > 100 
                      ? `${movie.description.substring(0, 100)}...` 
                      : movie.description}
                  </Card.Text>
                  
                  <h6 className="border-top pt-2 mb-3 d-flex align-items-center">
                    <i className="bi bi-calendar2-event me-2"></i>
                    Vetítések:
                  </h6>
                  {movieScreenings.length > 0 ? (
                    <div className="screenings-list">
                      {movieScreenings.map(screening => (
                        <div key={screening.id} className="screening-card mb-2">
                          <div className="d-flex justify-content-between align-items-center">
                            <div>
                              <div className="fw-bold">
                                {screening.date && screening.start_time ? 
                                  formatDate(screening.date, screening.start_time) : 
                                  formatDate(screening.start_time)}
                              </div>
                            </div>
                            <div onClick={(e) => e.stopPropagation()}>
                              {currentUser ? (
                                <Link to={`/book/${screening.id}`}>
                                  <Button variant="primary" size="sm">
                                    <i className="bi bi-ticket-perforated me-1"></i> Foglalás
                                  </Button>
                                </Link>
                              ) : (
                                <Button variant="primary" size="sm" disabled>
                                  <i className="bi bi-ticket-perforated me-1"></i> Foglalás
                                </Button>
                              )}
                              
                              {isAdmin() && (
                                <>
                                  <Link to={`/edit-screening/${screening.id}`}>
                                    <Button 
                                      variant="outline-warning" 
                                      size="sm" 
                                      className="ms-2"
                                    >
                                      <i className="bi bi-pencil"></i>
                                    </Button>
                                  </Link>
                                  <Button 
                                    variant="outline-danger" 
                                    size="sm" 
                                    className="ms-2"
                                    onClick={(e) => handleDeleteScreening(screening.id, e)}
                                  >
                                    <i className="bi bi-trash"></i>
                                  </Button>
                                </>
                              )}
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <div className="text-muted text-center p-3 bg-light rounded">
                      <i className="bi bi-calendar-x me-2"></i>
                      Nincs vetítés a kiválasztott időszakban
                    </div>
                  )}
                </Card.Body>
                {isAdmin() && (
                  <Card.Footer className="bg-white border-top">
                    <div className="d-flex justify-content-between" onClick={(e) => e.stopPropagation()}>
                      <Link to={`/edit-movie/${movie.id}`}>
                        <Button variant="warning" size="sm">
                          <i className="bi bi-pencil me-1"></i> Szerkesztés
                        </Button>
                      </Link>
                      <Button 
                        variant="danger" 
                        size="sm"
                        onClick={(e) => handleDeleteMovie(movie.id, e)}
                      >
                        <i className="bi bi-trash me-1"></i> Törlés
                      </Button>
                    </div>
                  </Card.Footer>
                )}
              </Card>
            );
          })}
        </div>
      )}
    </Container>
  );
};

export default HomePage; 