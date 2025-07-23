import api from './api';

const MovieService = {
  getAllMovies: async () => {
    try {
      const response = await api.get('/movies');
    
      const movies = response.data.data || response.data || [];
      return Array.isArray(movies) ? movies : [];
    } catch (error) {
      console.error('Movie service error:', error);
      throw error.response?.data || error;
    }
  },

  getMoviesByWeek: async (weekNumber, weekDay) => {
    try {
      if (!weekNumber) {
        return MovieService.getAllMovies();
      }

      const url = `/movies/week?week_number=${weekNumber}`;
      
      const response = await api.get(url);
      
      let movies = response.data.data || response.data || [];
      if (!Array.isArray(movies)) {
        console.error('Invalid movies response format:', movies);
        movies = [];
      }
      
      if (weekDay && movies.length > 0) {  
        
        movies = movies.map(movie => {
          if (movie.screenings && Array.isArray(movie.screenings)) {
            
            const filteredScreenings = movie.screenings.filter(screening => 
              parseInt(screening.week_day) === parseInt(weekDay)
            );

            return {
              ...movie,
              screenings: filteredScreenings
            };
          }
          return movie;
        });
        
        
        movies = movies.filter(movie => 
          movie.screenings && 
          Array.isArray(movie.screenings) && 
          movie.screenings.length > 0
        );
      }
      
      return movies;
    } catch (error) {
      console.error('Movie service error (week):', error);
      throw error.response?.data || error;
    }
  },

  getMovie: async (id) => {
    try {
      const response = await api.get(`/movies/${id}`);
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  getMovieWithScreenings: async (id, selectedWeek = null, selectedDay = null) => {
    try {
      
      const movie = await MovieService.getMovie(id);
      
      
      if (selectedWeek) {
        try {

          const weekUrl = `/movies/week?week_number=${selectedWeek}`;
          const weekResponse = await api.get(weekUrl);
          const weekMovies = weekResponse.data.data || weekResponse.data || [];
          
          
          const thisMovie = Array.isArray(weekMovies) 
            ? weekMovies.find(m => m.id === parseInt(id))
            : null;
            
          if (thisMovie && thisMovie.screenings && Array.isArray(thisMovie.screenings)) {
            let filteredScreenings = thisMovie.screenings;
            
            
            if (selectedDay) {
              filteredScreenings = filteredScreenings.filter(screening => 
                parseInt(screening.week_day) === parseInt(selectedDay)
              );
            }
            
            
            movie.screenings = filteredScreenings.map(screening => ({
              ...screening,
              week_number: parseInt(screening.week_number) || 0,
              week_day: parseInt(screening.week_day) || 0
            }));
            
            console.log(`Found ${movie.screenings.length} screenings for week ${selectedWeek}, day ${selectedDay || 'all'}`);
          } else {
            console.log(`Movie not found in week ${selectedWeek} data or has no screenings`);
            movie.screenings = [];
          }
        } catch (weekError) {
          console.error('Failed to fetch movie by week:', weekError);
          
        }
      } 
      
      
      if (!movie.screenings || !Array.isArray(movie.screenings) || movie.screenings.length === 0) {
        const response = await api.get('/screenings');
        console.log('API Response for screenings:', response);
        
        const allScreenings = response.data.data || response.data || [];
        
        if (Array.isArray(allScreenings)) {
          
          let movieScreenings = allScreenings.filter(s => s.movie_id === parseInt(id));
          
          
          if (selectedWeek) {
            movieScreenings = movieScreenings.filter(s => 
              parseInt(s.week_number) === parseInt(selectedWeek)
            );
            
            
            if (selectedDay) {
              movieScreenings = movieScreenings.filter(s => 
                parseInt(s.week_day) === parseInt(selectedDay)
              );
            }
          }
          
          
          movie.screenings = movieScreenings.map(screening => ({
            ...screening,
            week_number: parseInt(screening.week_number) || 0,
            week_day: parseInt(screening.week_day) || 0
          }));
          
          
          
          if (movie.screenings.some(s => !s.room || !s.room.name)) {
            try {
              const roomsResponse = await api.get('/rooms');
              const rooms = roomsResponse.data.data || roomsResponse.data || [];
              
              if (Array.isArray(rooms) && rooms.length > 0) {
                movie.screenings = movie.screenings.map(screening => {
                  if (screening.room_id) {
                    const room = rooms.find(r => r.id === screening.room_id);
                    if (room) {
                      screening.room = room;
                    } else if (!screening.room) {
                      screening.room = { name: `Terem ${screening.room_id}` };
                    }
                  } else if (!screening.room) {
                    screening.room = { name: 'N/A' };
                  }
                  return screening;
                });
              }
            } catch (roomError) {
              console.error('Failed to fetch rooms:', roomError);
            }
          }
        } else {
          movie.screenings = [];
        }
      } else if (movie.screenings && Array.isArray(movie.screenings)) {
        
        if (selectedWeek) {
          movie.screenings = movie.screenings.filter(s => 
            parseInt(s.week_number) === parseInt(selectedWeek)
          );
          
          if (selectedDay) {
            movie.screenings = movie.screenings.filter(s => 
              parseInt(s.week_day) === parseInt(selectedDay)
            );
          }
        }
        
        
        movie.screenings = movie.screenings.map(screening => ({
          ...screening,
          week_number: parseInt(screening.week_number) || 0,
          week_day: parseInt(screening.week_day) || 0
        }));
      }
      
      return movie;
    } catch (error) {
      console.error('Error fetching movie with screenings:', error);
      throw error.response?.data || error;
    }
  },

  createMovie: async (movieData) => {
    try {
      const response = await api.post('/movies', movieData);
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  updateMovie: async (id, movieData) => {
    try {
      const response = await api.put(`/movies/${id}`, movieData);
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  deleteMovie: async (id) => {
    try {
      const response = await api.delete(`/movies/${id}`);
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  }
};

export default MovieService; 